/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useEffect } from 'react';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';

interface LightboxProps {
  images: { imageUrl: string; title: string; description?: string }[];
  currentIndex: number | null;
  onClose: () => void;
  onPrev: () => void;
  onNext: () => void;
}

export default function GalleryLightbox({ images, currentIndex, onClose, onPrev, onNext }: LightboxProps) {
  useEffect(() => {
    if (currentIndex === null) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') onPrev();
      if (e.key === 'ArrowRight') onNext();
    };

    window.addEventListener('keydown', handleKeyDown);
    // Lock background scrolling while lightbox is active
    document.body.style.overflow = 'hidden';

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = '';
    };
  }, [currentIndex, onClose, onPrev, onNext]);

  if (currentIndex === null) return null;

  const currentImage = images[currentIndex];

  return (
    <div className="fixed inset-0 bg-black/95 z-55 flex flex-col justify-between p-4 md:p-8 animate-in fade-in duration-200">
      
      {/* Lightbox Header Controls */}
      <div className="flex justify-between items-center w-full relative z-10 text-white">
        <span className="text-xs font-semibold text-neutral-400">
          Image {currentIndex + 1} of {images.length}
        </span>
        <button
          onClick={onClose}
          className="p-2 rounded-full bg-neutral-900/80 hover:bg-neutral-800 text-white transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
          aria-label="Close Lightbox"
          id="lightbox-close-btn"
        >
          <X className="w-6 h-6" />
        </button>
      </div>

      {/* Main Image Stage */}
      <div className="flex-1 flex items-center justify-center relative my-4 max-h-[75vh]">
        
        {/* Previous Button */}
        <button
          onClick={onPrev}
          className="absolute left-2 md:left-4 p-3 rounded-full bg-neutral-900/80 hover:bg-neutral-800 text-white transition-all focus:outline-none focus:ring-2 focus:ring-blue-500 z-10 cursor-pointer"
          aria-label="Previous Image"
          id="lightbox-prev-btn"
        >
          <ChevronLeft className="w-6 h-6" />
        </button>

        {/* Central Display Image */}
        <img
          src={currentImage.imageUrl}
          alt={currentImage.title}
          referrerPolicy="no-referrer"
          className="max-w-full max-h-[70vh] object-contain rounded-lg select-none shadow-2xl transition-all"
        />

        {/* Next Button */}
        <button
          onClick={onNext}
          className="absolute right-2 md:right-4 p-3 rounded-full bg-neutral-900/80 hover:bg-neutral-800 text-white transition-all focus:outline-none focus:ring-2 focus:ring-blue-500 z-10 cursor-pointer"
          aria-label="Next Image"
          id="lightbox-next-btn"
        >
          <ChevronRight className="w-6 h-6" />
        </button>
      </div>

      {/* Caption Footer */}
      <div className="w-full text-center max-w-2xl mx-auto mb-4 relative z-10">
        <h4 className="text-sm md:text-base font-bold text-white tracking-tight">
          {currentImage.title}
        </h4>
        {currentImage.description && (
          <p className="text-xs md:text-sm text-neutral-400 mt-1 leading-relaxed">
            {currentImage.description}
          </p>
        )}
      </div>
    </div>
  );
}
