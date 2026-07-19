/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React from 'react';
import { PageId } from '../types';
import { BEFORE_AFTER_STORIES, BUSINESS_INFO } from '../data';
import { Check, Info, MessageSquare, Phone } from 'lucide-react';

interface BeforeAfterProps {
  onNavigate: (pageId: PageId) => void;
}

export default function BeforeAfter({ onNavigate }: BeforeAfterProps) {
  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Project Transformations</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Before & After
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Review side-by-side comparative logs detailing the immaculate swept-clean standards achieved by our local crew.
          </p>
        </div>
      </section>

      {/* Before & After Projects list */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 space-y-16">
        {BEFORE_AFTER_STORIES.map((story) => (
          <div 
            key={story.id}
            className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6 sm:p-10 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center"
            id={`before-after-card-${story.id}`}
          >
            {/* Split Images stage */}
            <div className="lg:col-span-8 grid grid-cols-1 sm:grid-cols-2 gap-4">
              {/* Before image */}
              <div className="relative rounded-xl overflow-hidden h-64 sm:h-80 border border-neutral-800">
                <img 
                  src={story.beforeImageUrl} 
                  alt={`${story.title} before collection`} 
                  className="w-full h-full object-cover opacity-75"
                  referrerPolicy="no-referrer"
                />
                <div className="absolute inset-0 bg-neutral-950/25"></div>
                <span className="absolute bottom-4 left-4 bg-red-600 text-white font-extrabold text-[10px] sm:text-xs py-1 px-3 rounded-full uppercase tracking-widest shadow-lg">
                  Before Collection
                </span>
              </div>

              {/* After image */}
              <div className="relative rounded-xl overflow-hidden h-64 sm:h-80 border border-neutral-800">
                <img 
                  src={story.afterImageUrl} 
                  alt={`${story.title} after swept clean`} 
                  className="w-full h-full object-cover"
                  referrerPolicy="no-referrer"
                />
                <div className="absolute inset-0 bg-neutral-950/5"></div>
                <span className="absolute bottom-4 left-4 bg-emerald-600 text-white font-extrabold text-[10px] sm:text-xs py-1 px-3 rounded-full uppercase tracking-widest shadow-lg">
                  Swept Clean
                </span>
              </div>
            </div>

            {/* Content particulars */}
            <div className="lg:col-span-4 flex flex-col justify-center">
              <div className="flex items-center space-x-2 text-xs font-bold uppercase text-blue-400 mb-1">
                <span>{story.category}</span>
                <span>•</span>
                <span className="text-neutral-500">{story.location}</span>
              </div>
              
              <h2 className="text-xl font-black text-white uppercase tracking-tight">
                {story.title}
              </h2>
              
              <p className="mt-3 text-neutral-400 text-xs sm:text-sm leading-relaxed">
                {story.description}
              </p>

              <div className="mt-5 space-y-2 text-xs font-semibold text-neutral-300 border-t border-neutral-800 pt-4">
                <span className="text-[10px] font-extrabold text-neutral-500 uppercase tracking-widest block mb-1">
                  Collection Results:
                </span>
                {story.details.map((detail, idx) => (
                  <div key={idx} className="flex items-start space-x-2">
                    <Check className="w-3.5 h-3.5 text-blue-500 shrink-0 mt-0.5" />
                    <span>{detail}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}
      </section>

      {/* Note about stock images if applicable */}
      <section className="py-12 bg-neutral-900/40 border-t border-b border-neutral-900 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mx-auto flex items-start space-x-3 bg-neutral-950 p-5 rounded-xl border border-neutral-800/80">
          <Info className="w-5 h-5 text-blue-500 shrink-0 mt-0.5" />
          <p className="text-xs text-neutral-400 leading-relaxed">
            <strong>Verification Policy:</strong> Supreme Waste Removal Services Ltd commits to absolute honesty. All before and after project cards published in this portal correspond to matched scenes loaded by our crew. We do not claim unrelated catalog graphics correspond to local locations.
          </p>
        </div>
      </section>

      {/* Conversion panel */}
      <section className="py-16 text-center">
        <div className="max-w-xl mx-auto px-4">
          <h2 className="text-xl sm:text-2xl font-extrabold text-white uppercase mb-2">Want your space transformed?</h2>
          <p className="text-xs sm:text-sm text-neutral-400 leading-relaxed mb-6">
            Clear pathways, empty garages, and pristine gardens are only a few clicks away. Shoot photos of your waste over WhatsApp to get your quote.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a 
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-3.5 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer shadow-lg shadow-emerald-950/10"
            >
              <MessageSquare className="w-4 h-4 mr-2" />
              WhatsApp Estimate
            </a>
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-900 hover:bg-neutral-800 text-white border border-neutral-800 py-3.5 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer"
            >
              <Phone className="w-4 h-4 mr-2 text-blue-400" />
              Call 07940 598 976
            </a>
          </div>
        </div>
      </section>

    </div>
  );
}
