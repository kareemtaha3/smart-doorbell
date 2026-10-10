'use client';

import React, { useState } from 'react';
import { Camera, Maximize2, X } from 'lucide-react';

interface VisitorPhotoCardProps {
  imageUrl?: string | null;
  timestamp: string;
  pressCount: number;
}

export const VisitorPhotoCard: React.FC<VisitorPhotoCardProps> = ({ imageUrl, timestamp, pressCount }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden group hover:border-slate-700 transition">
        <div className="aspect-video relative bg-slate-950 flex items-center justify-center overflow-hidden">
          {imageUrl ? (
            <>
              <img
                src={imageUrl}
                alt="Visitor snapshot"
                className="w-full h-full object-cover group-hover:scale-105 transition duration-300 cursor-pointer"
                onClick={() => setIsOpen(true)}
              />
              <button
                onClick={() => setIsOpen(true)}
                className="absolute top-2 right-2 bg-black/60 hover:bg-black/80 text-white p-1.5 rounded-lg opacity-0 group-hover:opacity-100 transition"
              >
                <Maximize2 className="h-4 w-4" />
              </button>
            </>
          ) : (
            <div className="text-center p-4 text-slate-500">
              <Camera className="h-8 w-8 mx-auto mb-1 opacity-50" />
              <span className="text-xs">No Snapshot Captured</span>
            </div>
          )}
        </div>

        <div className="p-3 flex items-center justify-between text-xs text-slate-400">
          <span>{new Date(timestamp).toLocaleString()}</span>
          <span className="bg-blue-950 text-blue-400 font-semibold px-2 py-0.5 rounded">
            {pressCount} Press(es)
          </span>
        </div>
      </div>

      {/* Lightbox Modal */}
      {isOpen && imageUrl && (
        <div className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center p-4 animate-in fade-in">
          <button
            onClick={() => setIsOpen(false)}
            className="absolute top-4 right-4 text-white bg-slate-800 hover:bg-slate-700 p-2 rounded-full"
          >
            <X className="h-6 w-6" />
          </button>
          <img src={imageUrl} alt="Visitor Snapshot Full View" className="max-w-full max-h-[90vh] object-contain rounded-lg" />
        </div>
      )}
    </>
  );
};

