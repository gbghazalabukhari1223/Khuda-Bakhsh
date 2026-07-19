/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId, GalleryItem } from '../types';
import { GALLERY_ITEMS } from '../data';
import { LayoutGrid, ZoomIn } from 'lucide-react';
import GalleryLightbox from '../components/GalleryLightbox';

interface GalleryProps {
  onNavigate: (pageId: PageId) => void;
}

type GalleryCategory = 'all' | 'vehicles' | 'household' | 'garden' | 'furniture' | 'builders' | 'commercial' | 'spaces';

export default function Gallery({ onNavigate }: GalleryProps) {
  const [activeTab, setActiveTab] = useState<GalleryCategory>('all');
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);

  // Filter gallery items
  const filteredItems = activeTab === 'all' 
    ? GALLERY_ITEMS 
    : GALLERY_ITEMS.filter(item => item.category === activeTab);

  const handlePrev = () => {
    if (lightboxIndex === null) return;
    setLightboxIndex(lightboxIndex === 0 ? filteredItems.length - 1 : lightboxIndex - 1);
  };

  const handleNext = () => {
    if (lightboxIndex === null) return;
    setLightboxIndex(lightboxIndex === filteredItems.length - 1 ? 0 : lightboxIndex + 1);
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Portfolio Showroom</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Work Gallery
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Authentic, high-resolution snapshots showing our vans, clearance operations, garden debris loads, and immaculate swept-clean results.
          </p>
        </div>
      </section>

      {/* Category Tab Filter Rail */}
      <section className="py-6 border-b border-neutral-900 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex flex-wrap gap-2 justify-center">
          {(['all', 'vehicles', 'household', 'garden', 'furniture', 'builders', 'commercial', 'spaces'] as GalleryCategory[]).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`py-2 px-4 rounded-lg text-xs font-bold uppercase tracking-wider transition-all cursor-pointer whitespace-nowrap ${
                activeTab === tab
                  ? 'bg-blue-600 text-white shadow-md'
                  : 'bg-neutral-900 text-neutral-400 hover:text-white hover:bg-neutral-800'
              }`}
              id={`gallery-tab-${tab}`}
            >
              {tab === 'all' && 'All Photos'}
              {tab === 'vehicles' && 'Our Vehicles'}
              {tab === 'household' && 'House Clearances'}
              {tab === 'garden' && 'Gardens'}
              {tab === 'furniture' && 'Furniture'}
              {tab === 'builders' && 'Builders Debris'}
              {tab === 'commercial' && 'Commercial'}
              {tab === 'spaces' && 'Cleared Spaces'}
            </button>
          ))}
        </div>
      </section>

      {/* Grid Display */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {filteredItems.map((item, idx) => (
            <div
              key={item.id}
              onClick={() => setLightboxIndex(idx)}
              className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden cursor-pointer group hover:border-blue-500/30 transition-all duration-300 relative"
              id={`gallery-item-card-${item.id}`}
            >
              {/* Image block */}
              <div className="h-56 overflow-hidden relative">
                <img
                  src={item.imageUrl}
                  alt={item.title}
                  className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-104"
                  referrerPolicy="no-referrer"
                />
                <div className="absolute inset-0 bg-neutral-950/0 group-hover:bg-neutral-950/40 transition-colors duration-300 flex items-center justify-center">
                  <div className="p-3 rounded-full bg-blue-600 text-white opacity-0 group-hover:opacity-100 scale-90 group-hover:scale-100 transition-all duration-300">
                    <ZoomIn className="w-5 h-5" />
                  </div>
                </div>
              </div>

              {/* Caption details */}
              <div className="p-4 bg-neutral-900/60">
                <span className="text-[10px] font-bold text-blue-400 uppercase tracking-wider block mb-1">
                  {item.category}
                </span>
                <h3 className="text-xs font-bold text-white uppercase tracking-tight truncate">
                  {item.title}
                </h3>
                <p className="text-[11px] text-neutral-400 mt-1 line-clamp-2">
                  {item.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Interactive Lightbox Portal */}
      {lightboxIndex !== null && (
        <GalleryLightbox
          images={filteredItems}
          currentIndex={lightboxIndex}
          onClose={() => setLightboxIndex(null)}
          onPrev={handlePrev}
          onNext={handleNext}
        />
      )}

    </div>
  );
}
