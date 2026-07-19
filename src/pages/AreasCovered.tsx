/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId } from '../types';
import { AREAS_COVERED, BUSINESS_INFO } from '../data';
import { MapPin, Info, MessageSquare, Phone, Check } from 'lucide-react';

interface AreasCoveredProps {
  onNavigate: (pageId: PageId) => void;
}

export default function AreasCovered({ onNavigate }: AreasCoveredProps) {
  const [postcode, setPostcode] = useState('');
  const [status, setStatus] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!postcode) return;
    
    const formatted = postcode.trim().toUpperCase();
    const match = AREAS_COVERED.find(
      area => formatted.includes(area.name.toUpperCase()) || area.postcode.split(', ').some(p => formatted.startsWith(p))
    );

    if (match) {
      setStatus(`✅ Service Confirmed! We have scheduled routes in ${match.name} (${match.postcode}) with status: ${match.availability}.`);
    } else {
      setStatus(`⚡ We regularly service Devon & Cornwall! Let's verify active routes for "${formatted}" on WhatsApp.`);
    }
  };

  const getWaLink = () => {
    const text = `Hello Supreme Waste Removal, I'd like to check availability for postcode: ${postcode || '(Postcode)'}`;
    return `https://wa.me/447940598976?text=${encodeURIComponent(text)}`;
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Devon & East Cornwall</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Areas Covered
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Waste Removal Across Plymouth and Nearby Suburbs. Check active schedules for your collection postcode.
          </p>
        </div>
      </section>

      {/* Main postcodes list & Checker */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-12 gap-12">
        
        {/* Left Column (Locations details) */}
        <div className="lg:col-span-7 space-y-8">
          <div>
            <h2 className="text-xl font-black text-white uppercase tracking-tight mb-4 border-b border-neutral-900 pb-2">
              Our Active Service Network
            </h2>
            <p className="text-neutral-400 text-sm sm:text-base leading-relaxed mb-6">
              Supreme Waste Removal Services Ltd operates centrally from <strong>53 Duncombe Avenue in Plymouth</strong>. To maintain rapid morning and afternoon slots, we structure daily scheduled routes across the following districts:
            </p>

            <div className="space-y-4">
              {AREAS_COVERED.map((area, idx) => (
                <div key={idx} className="p-4 bg-neutral-900 rounded-xl border border-neutral-850 flex items-start justify-between gap-4">
                  <div className="flex items-start space-x-3">
                    <MapPin className="w-5 h-5 text-blue-500 shrink-0 mt-0.5" />
                    <div>
                      <h3 className="text-sm font-bold text-white uppercase tracking-wider">{area.name}</h3>
                      <p className="text-xs text-neutral-400 mt-0.5">Postcodes: {area.postcode}</p>
                    </div>
                  </div>
                  <span className="text-[10px] font-extrabold text-emerald-400 uppercase bg-emerald-500/10 border border-emerald-500/20 px-2 py-1 rounded-full shrink-0">
                    {area.availability}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-neutral-900/30 border border-neutral-850 p-6 rounded-xl flex items-start space-x-4">
            <Info className="w-5 h-5 text-blue-500 shrink-0 mt-0.5" />
            <div>
              <h4 className="text-xs font-bold text-white uppercase tracking-widest mb-1">Route & Availability Policies</h4>
              <p className="text-xs text-neutral-400 leading-relaxed">
                We make absolute commitments to avoid establishing false office addresses in adjacent areas. All operations are dispatched centrally from Plymouth. Our service capabilities depend heavily on: (1) collection scale, (2) accessibility of waste pile, and (3) current daily van schedules.
              </p>
            </div>
          </div>
        </div>

        {/* Right Column (Postcode Checker Form) */}
        <div className="lg:col-span-5">
          <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6 relative overflow-hidden">
            <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 rounded-full blur-2xl"></div>
            
            <span className="text-[10px] font-extrabold text-blue-400 uppercase tracking-widest block mb-1">
              Availability Checker
            </span>
            <h3 className="text-lg font-black text-white uppercase tracking-tight">
              Check Your Postcode
            </h3>
            <p className="mt-2 text-xs text-neutral-400 leading-relaxed mb-6">
              Insert your Plymouth suburb or postcode below to verify if we have active vehicles operating in your area today.
            </p>

            <form onSubmit={handleSubmit} className="space-y-4">
              <input
                type="text"
                placeholder="Insert Postcode (e.g. PL5) *"
                required
                value={postcode}
                onChange={(e) => setPostcode(e.target.value)}
                className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors uppercase"
                id="areas-postcode-input"
              />
              <button
                type="submit"
                className="w-full bg-blue-600 hover:bg-blue-500 text-white font-extrabold text-xs py-3 rounded-lg transition-colors cursor-pointer text-center"
                id="areas-postcode-submit"
              >
                Verify Coverage
              </button>
            </form>

            {status && (
              <div className="mt-6 p-4 bg-neutral-950/60 border border-neutral-850 rounded-xl text-xs leading-relaxed animate-in fade-in duration-200">
                <p className="text-neutral-300 font-semibold mb-3">{status}</p>
                <div className="flex flex-col sm:flex-row gap-2">
                  <a
                    href={getWaLink()}
                    target="_blank"
                    rel="noreferrer"
                    className="flex-1 bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-2.5 px-3 rounded-lg text-center cursor-pointer flex items-center justify-center text-[11px]"
                    id="areas-wa-btn"
                  >
                    <MessageSquare className="w-3.5 h-3.5 mr-1.5" />
                    Confirm on WhatsApp
                  </a>
                  <a
                    href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
                    className="flex-1 bg-neutral-800 hover:bg-neutral-750 text-white font-bold py-2.5 px-3 rounded-lg text-center cursor-pointer flex items-center justify-center text-[11px] border border-neutral-700"
                  >
                    <Phone className="w-3.5 h-3.5 mr-1.5 text-blue-400" />
                    Call 07940 598 976
                  </a>
                </div>
              </div>
            )}
          </div>
        </div>
      </section>

    </div>
  );
}
