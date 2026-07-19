/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useMemo } from 'react';
import { PageId, Service } from '../types';
import { SERVICES, BUSINESS_INFO } from '../data';
import { Search, ChevronRight, MessageSquare, Check } from 'lucide-react';

interface ServicesProps {
  onNavigate: (pageId: PageId) => void;
}

type FilterCategory = 'all' | 'domestic' | 'outdoor' | 'commercial' | 'flexible';

export default function Services({ onNavigate }: ServicesProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<FilterCategory>('all');

  const handleServiceClick = (serviceId: PageId) => {
    onNavigate(serviceId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Map service ID to customer need categorizations
  const getServiceCategoryType = (id: PageId): FilterCategory => {
    if (['house-clearance-plymouth', 'rubbish-removal-plymouth', 'waste-removal-plymouth', 'furniture-removal-plymouth', 'garage-clearance-plymouth'].includes(id)) {
      return 'domestic';
    }
    if (['garden-waste-removal-plymouth', 'builders-waste-removal-plymouth'].includes(id)) {
      return 'outdoor';
    }
    if (['commercial-waste-removal-plymouth', 'office-clearance-plymouth'].includes(id)) {
      return 'commercial';
    }
    return 'flexible'; // man-and-van, recycling, same-day
  };

  // Filter & Search computation
  const filteredServices = useMemo(() => {
    return SERVICES.filter(srv => {
      // Filter by category type
      if (activeFilter !== 'all' && getServiceCategoryType(srv.id) !== activeFilter) {
        return false;
      }
      // Search by text query
      if (searchQuery) {
        const query = searchQuery.toLowerCase();
        return (
          srv.title.toLowerCase().includes(query) ||
          srv.shortDesc.toLowerCase().includes(query) ||
          srv.description.toLowerCase().includes(query) ||
          srv.items.some(item => item.toLowerCase().includes(query))
        );
      }
      return true;
    });
  }, [searchQuery, activeFilter]);

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Banner Intro */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Service Catalog</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Clearance Solutions
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Search or filter through our twelve professional waste removal services to find the precise option for your needs.
          </p>
        </div>
      </section>

      {/* Filter and Search Bar Control */}
      <section className="py-8 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 border-b border-neutral-900">
        <div className="flex flex-col md:flex-row items-center justify-between gap-6">
          {/* Category Navigation tabs */}
          <div className="flex flex-wrap gap-2 justify-center">
            {(['all', 'domestic', 'outdoor', 'commercial', 'flexible'] as FilterCategory[]).map((cat) => (
              <button
                key={cat}
                onClick={() => setActiveFilter(cat)}
                className={`py-2 px-4 rounded-lg text-xs font-bold uppercase tracking-wider transition-all cursor-pointer ${
                  activeFilter === cat
                    ? 'bg-blue-600 text-white shadow-md'
                    : 'bg-neutral-900 text-neutral-400 hover:text-white hover:bg-neutral-800'
                }`}
                id={`filter-tab-${cat}`}
              >
                {cat === 'all' && 'All Services (12)'}
                {cat === 'domestic' && 'Domestic Clearance'}
                {cat === 'outdoor' && 'Outdoor & Builders'}
                {cat === 'commercial' && 'Commercial & Offices'}
                {cat === 'flexible' && 'Flexible Collections'}
              </button>
            ))}
          </div>

          {/* Search Input */}
          <div className="relative w-full md:w-80">
            <input
              type="text"
              placeholder="Search services..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full bg-neutral-900 border border-neutral-800 text-xs rounded-lg pl-10 pr-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
              id="services-search-input"
            />
            <Search className="w-4 h-4 text-neutral-500 absolute left-3 top-3.5" />
          </div>
        </div>
      </section>

      {/* Directory visual lists */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {filteredServices.length === 0 ? (
          <div className="text-center py-12 bg-neutral-900/40 rounded-xl border border-neutral-850">
            <p className="text-neutral-400 text-sm">No services matched your search keywords or filter category.</p>
            <button 
              onClick={() => { setSearchQuery(''); setActiveFilter('all'); }} 
              className="mt-4 text-xs font-bold text-blue-500 hover:text-blue-400 uppercase tracking-widest cursor-pointer"
              id="reset-filter-btn"
            >
              Reset Filters
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {filteredServices.map((srv) => (
              <div 
                key={srv.id}
                className="bg-neutral-900/60 border border-neutral-800/80 rounded-xl overflow-hidden flex flex-col justify-between hover:border-blue-500/30 transition-all duration-300 group"
                id={`services-directory-card-${srv.id}`}
              >
                <div>
                  {/* Service Card Image */}
                  <div className="h-48 overflow-hidden relative">
                    <img 
                      src={srv.imageUrl} 
                      alt={srv.title} 
                      className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-103"
                      referrerPolicy="no-referrer"
                    />
                    <div className="absolute inset-0 bg-neutral-950/20"></div>
                    <span className="absolute bottom-3 left-3 bg-neutral-950/80 text-white text-[10px] font-bold uppercase tracking-widest py-1 px-2.5 rounded-full border border-neutral-800">
                      {srv.category}
                    </span>
                  </div>

                  {/* Card Info Content */}
                  <div className="p-6">
                    <span className="text-[10px] font-extrabold text-blue-400 uppercase tracking-wider block mb-1">
                      Service {srv.number}
                    </span>
                    <h2 className="text-base font-black text-white uppercase tracking-tight group-hover:text-blue-400 transition-colors">
                      {srv.title.replace(' Plymouth', '')}
                    </h2>
                    <p className="mt-3 text-neutral-400 text-xs leading-relaxed line-clamp-3">
                      {srv.shortDesc}
                    </p>

                    <div className="mt-4 space-y-1 text-[11px] text-neutral-300 border-t border-neutral-850 pt-3">
                      {srv.items.slice(0, 3).map((item, idx) => (
                        <div key={idx} className="flex items-center space-x-1.5">
                          <Check className="w-3.5 h-3.5 text-blue-500 shrink-0" />
                          <span className="truncate">{item}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>

                {/* Card Button footer */}
                <div className="p-6 pt-0 border-t border-neutral-900/60 mt-2 flex justify-between items-center bg-neutral-900/20">
                  <button
                    onClick={() => handleServiceClick(srv.id)}
                    className="text-xs font-bold text-white group-hover:text-blue-400 transition-colors uppercase tracking-wider flex items-center cursor-pointer"
                    id={`directory-btn-view-${srv.id}`}
                  >
                    View Info
                    <ChevronRight className="w-3.5 h-3.5 ml-1" />
                  </button>
                  <a
                    href={BUSINESS_INFO.whatsappLink}
                    target="_blank"
                    rel="noreferrer"
                    className="text-xs font-bold text-emerald-400 hover:text-emerald-300 transition-colors flex items-center cursor-pointer"
                    id={`directory-btn-whatsapp-${srv.id}`}
                  >
                    Quote
                    <MessageSquare className="w-3.5 h-3.5 ml-1" />
                  </a>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Pre-Footer Action */}
      <section className="bg-neutral-900 py-16 text-center border-t border-neutral-850">
        <div className="max-w-xl mx-auto px-4">
          <h2 className="text-xl sm:text-2xl font-extrabold text-white uppercase">Not sure which service is suitable?</h2>
          <p className="mt-2 text-xs sm:text-sm text-neutral-400">
            Submit a snapshot of your clutter over WhatsApp, and we will advise on the best clearance approach.
          </p>
          <a
            href={BUSINESS_INFO.whatsappLink}
            target="_blank"
            rel="noreferrer"
            className="mt-6 bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-bold uppercase tracking-widest py-3.5 px-6 rounded-lg inline-flex items-center cursor-pointer"
          >
            <MessageSquare className="w-4 h-4 mr-2" />
            Quick Consultation Chat
          </a>
        </div>
      </section>

    </div>
  );
}
