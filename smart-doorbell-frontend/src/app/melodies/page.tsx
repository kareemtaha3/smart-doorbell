'use client';

import React, { useEffect, useState } from 'react';
import { Music, Upload, Star, Volume2, CheckCircle2 } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useHome } from '@/context/HomeContext';
import { melodiesApi } from '@/lib/api';
import { Melody } from '@/lib/types';

export default function MelodiesPage() {
  const { user } = useAuth();
  const { activeHome } = useHome();
  const [melodies, setMelodies] = useState<Melody[]>([]);
  const [loading, setLoading] = useState(true);

  // Form State
  const [name, setName] = useState('');
  const [isMain, setIsMain] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  const fetchMelodies = async () => {
    if (!activeHome) return;
    setLoading(true);
    try {
      const list = await melodiesApi.getHomeMelodies(activeHome.id);
      setMelodies(list);
    } catch (err) {
      console.error('Failed to fetch melodies:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMelodies();
  }, [activeHome?.id]);

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !file || !activeHome || !user) return;

    setUploading(true);
    try {
      await melodiesApi.uploadMelody(activeHome.id, user.id, name.trim(), isMain, file);
      setName('');
      setIsMain(false);
      setFile(null);
      await fetchMelodies();
      alert('Melody uploaded successfully to MinIO cloud!');
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      console.error('Error uploading melody:', err);
      alert(`Upload error: ${errorMsg}`);
    } finally {
      setUploading(false);
    }
  };

  const handleSetMain = async (melodyId: string) => {
    if (!activeHome || !user) return;
    try {
      await melodiesApi.setMainMelody(activeHome.id, melodyId, user.id);
      await fetchMelodies();
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      alert(`Error setting main melody: ${errorMsg}`);
    }
  };

  if (!activeHome) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-10 text-center max-w-lg mx-auto space-y-3">
        <Music className="h-10 w-10 text-slate-600 mx-auto" />
        <h2 className="text-lg font-bold text-white">No Active Home</h2>
        <p className="text-sm text-slate-400">Please select an active home to manage chime melodies.</p>
      </div>
    );
  }

  return (
    <div className="space-y-8 max-w-5xl mx-auto">
      <div>
        <h1 className="text-2xl font-extrabold text-white flex items-center space-x-2">
          <Music className="h-7 w-7 text-blue-500" />
          <span>Indoor Chime Melodies</span>
        </h1>
        <p className="text-slate-400 text-sm">
          Upload custom audio tones to MinIO cloud and set the main melody for <strong className="text-blue-400">{activeHome.name}</strong>.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        
        {/* Left Column: Upload Form */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4 shadow-lg h-fit">
          <h2 className="text-base font-bold text-slate-200 flex items-center space-x-2">
            <Upload className="h-5 w-5 text-blue-400" />
            <span>Upload New Chime Tone</span>
          </h2>

          <form onSubmit={handleUpload} className="space-y-4">
            <div>
              <label className="text-xs font-semibold text-slate-400 block mb-1">Tone Title</label>
              <input
                type="text"
                placeholder="e.g. Classic Westminster, Gentle Bell"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full bg-slate-950 border border-slate-700 text-slate-100 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>

            <div>
              <label className="text-xs font-semibold text-slate-400 block mb-1">Audio File (.mp3, .wav)</label>
              <input
                type="file"
                accept="audio/*"
                onChange={(e) => setFile(e.target.files?.[0] || null)}
                className="w-full text-xs text-slate-400 file:mr-3 file:py-2 file:px-4 file:rounded-xl file:border-0 file:text-xs file:font-semibold file:bg-slate-800 file:text-slate-200 hover:file:bg-slate-700 cursor-pointer"
                required
              />
            </div>

            <div className="flex items-center space-x-2 pt-1">
              <input
                type="checkbox"
                id="isMain"
                checked={isMain}
                onChange={(e) => setIsMain(e.target.checked)}
                className="h-4 w-4 rounded bg-slate-950 border-slate-700 text-blue-600 focus:ring-blue-500"
              />
              <label htmlFor="isMain" className="text-xs text-slate-300 font-medium cursor-pointer">
                Set as Main Home Melody immediately
              </label>
            </div>

            <button
              type="submit"
              disabled={uploading}
              className="w-full bg-blue-600 hover:bg-blue-500 text-white font-semibold py-2.5 rounded-xl text-sm transition disabled:opacity-50"
            >
              {uploading ? 'Uploading to MinIO...' : 'Upload Melody File'}
            </button>
          </form>
        </div>

        {/* Right Column: Melodies Gallery */}
        <div className="md:col-span-2 space-y-4">
          <h2 className="text-base font-bold text-slate-200">Configured Melodies ({melodies.length})</h2>

          {loading ? (
            <p className="text-xs text-slate-400">Loading melodies...</p>
          ) : melodies.length > 0 ? (
            <div className="space-y-4">
              {melodies.map((melody) => (
                <div
                  key={melody.id}
                  className={`bg-slate-900 border rounded-2xl p-5 space-y-3 transition ${
                    melody.isMain ? 'border-amber-500/80 ring-1 ring-amber-500/30' : 'border-slate-800'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="p-2.5 bg-slate-950 rounded-xl text-blue-400 border border-slate-800">
                        <Volume2 className="h-5 w-5" />
                      </div>
                      <div>
                        <h3 className="font-bold text-slate-200 text-sm flex items-center space-x-2">
                          <span>{melody.name}</span>
                          {melody.isMain && (
                            <span className="bg-amber-500/20 text-amber-300 text-[10px] font-bold px-2 py-0.5 rounded-full border border-amber-500/40 flex items-center space-x-1">
                              <Star className="h-3 w-3 fill-amber-400" />
                              <span>MAIN MELODY</span>
                            </span>
                          )}
                        </h3>
                        <span className="text-[11px] text-slate-400">Uploaded {new Date(melody.createdAt).toLocaleDateString()}</span>
                      </div>
                    </div>

                    {!melody.isMain && (
                      <button
                        onClick={() => handleSetMain(melody.id)}
                        className="flex items-center space-x-1.5 bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold px-3 py-1.5 rounded-lg transition"
                      >
                        <CheckCircle2 className="h-3.5 w-3.5 text-amber-400" />
                        <span>Set as Main</span>
                      </button>
                    )}
                  </div>

                  {/* Audio Preview Player */}
                  <div className="bg-slate-950 p-2 rounded-xl border border-slate-800">
                    <audio controls src={melody.toneFileUrl} className="w-full h-8 rounded-lg" />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="bg-slate-900/50 border border-slate-800 rounded-2xl p-8 text-center text-slate-400 text-xs space-y-2">
              <Music className="h-8 w-8 mx-auto text-slate-600" />
              <p>No custom melodies uploaded for this home yet.</p>
            </div>
          )}
        </div>

      </div>
    </div>
  );
}

