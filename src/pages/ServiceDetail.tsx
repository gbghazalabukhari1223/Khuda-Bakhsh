/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect } from 'react';
import { PageId, Service } from '../types';
import { SERVICES, BUSINESS_INFO } from '../data';
import { Check, Info, Phone, ChevronDown, ChevronRight, MessageSquare } from 'lucide-react';

interface ServiceDetailProps {
  serviceId: PageId;
  onNavigate: (pageId: PageId) => void;
}

export default function ServiceDetail({ serviceId, onNavigate }: ServiceDetailProps) {
  const [openFaqIdx, setOpenFaqIdx] = useState<number | null>(0);

  // Retrieve service data
  const service = SERVICES.find(s => s.id === serviceId);

  // Reset FAQ expand index when serviceId changes
  useEffect(() => {
    setOpenFaqIdx(0);
  }, [serviceId]);

  if (!service) {
    return (
      <div className="py-24 text-center bg-neutral-950 text-white">
        <p className="text-neutral-400 text-sm">Clearance service page not found.</p>
        <button 
          onClick={() => onNavigate('services')} 
          className="mt-4 bg-blue-600 hover:bg-blue-500 py-2 px-4 rounded text-xs font-bold"
        >
          Back to Directory
        </button>
      </div>
    );
  }

  // Pick complementary services as related links
  const relatedServices = SERVICES.filter(s => s.id !== serviceId).slice(0, 2);

  const handleRelatedClick = (relatedId: PageId) => {
    onNavigate(relatedId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-service-page">
      
      {/* 1. SERVICE-SPECIFIC HERO BANNER */}
      <section className="relative min-h-[50vh] flex items-center justify-center py-20 px-4 sm:px-6 lg:px-8 overflow-hidden border-b border-neutral-900">
        <div className="absolute inset-0">
          <img 
            src={service.imageUrl} 
            alt={service.title} 
            className="w-full h-full object-cover object-center opacity-30"
            referrerPolicy="no-referrer"
          />
          <div className="absolute inset-0 bg-neutral-950/80"></div>
          <div className="absolute inset-0 bg-gradient-to-t from-neutral-950 via-transparent to-transparent"></div>
        </div>

        <div className="max-w-4xl mx-auto text-center relative z-10 flex flex-col items-center">
          <span className="inline-flex items-center text-xs font-bold uppercase tracking-widest text-blue-400 mb-4 bg-blue-500/10 border border-blue-500/20 px-3 py-1 rounded-full">
            Service {service.number} • Plymouth Division
          </span>
          {/* Unique H1 */}
          <h1 className="text-3xl sm:text-5xl font-black tracking-tight text-white uppercase max-w-3xl leading-none">
            {service.title}
          </h1>
          <p className="mt-4 text-neutral-300 text-xs sm:text-base max-w-2xl leading-relaxed">
            {service.shortDesc}
          </p>
        </div>
      </section>

      {/* 2. LOCAL INTRO & BREADCRUMBS */}
      <section className="py-6 bg-neutral-900/40 border-b border-neutral-900 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-3 text-xs text-neutral-500">
          {/* Breadcrumbs */}
          <div className="flex items-center space-x-1.5">
            <button onClick={() => onNavigate('home')} className="hover:text-blue-400 transition-colors">Home</button>
            <span>/</span>
            <button onClick={() => onNavigate('services')} className="hover:text-blue-400 transition-colors">Services</button>
            <span>/</span>
            <span className="text-neutral-300 font-medium">{service.title.replace(' Plymouth', '')}</span>
          </div>

          <span className="text-emerald-500 font-semibold flex items-center">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 mr-2 animate-pulse"></span>
            Bookings active in your postcode today
          </span>
        </div>
      </section>

      {/* 3. MAIN SERVICE DETAILS CONTENT */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-12 gap-12">
        {/* Left main grid (Content rich details) */}
        <div className="lg:col-span-8 space-y-12">
          {/* Deep Plymouth Introduction */}
          <div>
            <h2 className="text-lg font-extrabold text-white uppercase tracking-tight mb-4 border-b border-neutral-900 pb-2">
              Plymouth District Clearance Services
            </h2>
            <p className="text-neutral-400 text-sm sm:text-base leading-relaxed">
              {service.description}
            </p>
          </div>

          {/* What the Service Includes */}
          <div className="bg-neutral-900/60 border border-neutral-800 p-6 sm:p-8 rounded-xl">
            <h2 className="text-sm font-bold text-blue-400 uppercase tracking-widest mb-4 flex items-center">
              <span className="w-1 h-4 bg-blue-500 mr-2 rounded-full"></span>
              What this clearance includes
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs sm:text-sm text-neutral-300">
              {service.items.map((item, idx) => (
                <div key={idx} className="flex items-start space-x-2.5">
                  <Check className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" />
                  <span>{item}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Core Service features */}
          <div>
            <h2 className="text-base font-extrabold text-white uppercase tracking-tight mb-4">
              Our Professional Standards
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs sm:text-sm text-neutral-400">
              {service.features.map((feat, idx) => (
                <div key={idx} className="p-4 bg-neutral-900 rounded-lg border border-neutral-850 flex items-start space-x-3">
                  <span className="w-5 h-5 rounded-full bg-blue-500/10 border border-blue-500/20 text-blue-400 flex items-center justify-center shrink-0 font-bold text-xs mt-0.5">
                    {idx + 1}
                  </span>
                  <span>{feat}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Who is it suitable for */}
          <div>
            <h2 className="text-base font-extrabold text-white uppercase tracking-tight mb-4">
              Who is this service suitable for?
            </h2>
            <ul className="space-y-2.5 text-xs sm:text-sm text-neutral-400">
              {service.whoIsItFor.map((item, idx) => (
                <li key={idx} className="flex items-start space-x-2">
                  <span className="text-blue-500 shrink-0 mt-1">•</span>
                  <span>{item}</span>
                </li>
              ))}
            </ul>
          </div>

          {/* Access and collection considerations */}
          <div className="bg-neutral-900/30 border border-neutral-850 p-6 rounded-xl flex items-start space-x-4">
            <Info className="w-5 h-5 text-amber-500 shrink-0 mt-1" />
            <div>
              <h3 className="text-sm font-bold text-white uppercase tracking-wider mb-2">Access & Loading Considerations</h3>
              <ul className="space-y-1.5 text-xs text-neutral-400 leading-relaxed">
                {service.accessConsiderations.map((cons, idx) => (
                  <li key={idx}>- {cons}</li>
                ))}
              </ul>
            </div>
          </div>
        </div>

        {/* Right side panel (Related links, dynamic quoting widget) */}
        <div className="lg:col-span-4 space-y-8">
          {/* WhatsApp Quote Box */}
          <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6 relative overflow-hidden">
            <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 rounded-full blur-2xl"></div>
            
            <span className="text-[10px] font-extrabold text-blue-400 uppercase tracking-widest block mb-1">
              On-Demand Estimator
            </span>
            <h3 className="text-lg font-black text-white uppercase tracking-tight">
              Request a fast quote
            </h3>
            <p className="mt-2 text-xs text-neutral-400 leading-relaxed mb-6">
              Send pictures of your pile directly through WhatsApp with your Plymouth postcode for a prompt, competitive price.
            </p>

            <div className="space-y-3">
              <a
                href={BUSINESS_INFO.whatsappLink}
                target="_blank"
                rel="noreferrer"
                className="w-full bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold text-center py-3.5 px-4 rounded-xl text-xs flex items-center justify-center transition-colors cursor-pointer shadow-lg shadow-emerald-950/15"
                id="detail-whatsapp-btn"
              >
                <MessageSquare className="w-4 h-4 mr-2" />
                Quote via WhatsApp
              </a>
              <a
                href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
                className="w-full bg-neutral-800 hover:bg-neutral-700 border border-neutral-700 text-white font-extrabold text-center py-3.5 px-4 rounded-xl text-xs flex items-center justify-center transition-colors cursor-pointer"
                id="detail-phone-btn"
              >
                <Phone className="w-4 h-4 mr-2 text-blue-500" />
                Call 07940 598 976
              </a>
            </div>
          </div>

          {/* Related Services Links */}
          <div className="bg-neutral-900/60 border border-neutral-850 p-6 rounded-2xl">
            <h3 className="text-xs font-bold text-neutral-400 uppercase tracking-widest mb-4">
              Other Services In Plymouth
            </h3>
            <div className="space-y-4">
              {relatedServices.map(srv => (
                <button
                  key={srv.id}
                  onClick={() => handleRelatedClick(srv.id)}
                  className="w-full text-left p-3 rounded-lg hover:bg-neutral-800/50 border border-neutral-850 hover:border-blue-500/20 transition-all flex items-center space-x-3 group"
                  id={`detail-related-${srv.id}`}
                >
                  <img 
                    src={srv.imageUrl} 
                    alt={srv.title} 
                    className="w-12 h-12 object-cover rounded-md shrink-0"
                    referrerPolicy="no-referrer"
                  />
                  <div className="overflow-hidden">
                    <div className="text-xs font-bold text-white group-hover:text-blue-400 transition-colors truncate">
                      {srv.title.replace(' Plymouth', '')}
                    </div>
                    <div className="text-[10px] text-neutral-400 truncate mt-0.5">
                      {srv.category}
                    </div>
                  </div>
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* 4. SERVICE-SPECIFIC FAQs ACCORDION */}
      <section className="py-16 bg-neutral-900/40 border-t border-b border-neutral-900">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-2">Service FAQs</span>
            <h2 className="text-xl sm:text-2xl font-extrabold text-white uppercase">Service Specific Questions</h2>
          </div>

          <div className="space-y-3">
            {service.faqs.map((faq, idx) => {
              const isOpen = openFaqIdx === idx;
              return (
                <div key={idx} className="bg-neutral-950 border border-neutral-800 rounded-xl overflow-hidden">
                  <button
                    onClick={() => setOpenFaqIdx(isOpen ? null : idx)}
                    className="w-full flex justify-between items-center text-left p-4 text-xs sm:text-sm font-bold text-white hover:text-blue-400 transition-colors focus:outline-none"
                    aria-expanded={isOpen}
                    id={`detail-faq-btn-${idx}`}
                  >
                    <span>{faq.question}</span>
                    <ChevronDown className={`w-4 h-4 text-neutral-500 shrink-0 ml-4 transition-transform duration-200 ${isOpen ? 'rotate-185 text-blue-500' : 'rotate-0'}`} />
                  </button>

                  {isOpen && (
                    <div className="px-4 pb-4 pt-0 border-t border-neutral-900 text-xs text-neutral-400 leading-relaxed animate-in fade-in duration-200">
                      {faq.answer}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </section>

    </div>
  );
}
