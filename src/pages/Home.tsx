/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId, Service } from '../types';
import { BUSINESS_INFO, SERVICES, AREAS_COVERED, GENERAL_FAQS, BEFORE_AFTER_STORIES } from '../data';
import vanImg from '../assets/van.jpg';
import { 
  Phone, 
  ChevronRight, 
  Check, 
  MapPin, 
  Calendar, 
  HelpCircle, 
  ChevronDown, 
  Sparkles, 
  Trash2, 
  ShieldCheck, 
  Recycle, 
  MessageSquare,
  Zap,
  ArrowRight
} from 'lucide-react';

interface HomeProps {
  onNavigate: (pageId: PageId) => void;
}

export default function Home({ onNavigate }: HomeProps) {
  // Accordion active index
  const [openFaqIndex, setOpenFaqIndex] = useState<number | null>(0);
  
  // Custom interactive check for postcode
  const [checkedPostcode, setCheckedPostcode] = useState('');
  const [postcodeStatus, setPostcodeStatus] = useState<string | null>(null);

  // Before & After Active Case Study Tab
  const [activeBaIdx, setActiveBaIdx] = useState(0);

  const handlePostcodeCheck = (e: React.FormEvent) => {
    e.preventDefault();
    if (!checkedPostcode) return;
    
    const formatted = checkedPostcode.trim().toUpperCase();
    const isMatched = AREAS_COVERED.some(
      area => formatted.includes(area.name.toUpperCase()) || area.postcode.split(', ').some(p => formatted.startsWith(p))
    );

    if (isMatched) {
      setPostcodeStatus("✅ Yes! We have active teams in your postcode today.");
    } else {
      setPostcodeStatus("⚡ We cover that area! Let's double check route schedules on WhatsApp.");
    }
  };

  const getPostcodeWhatsAppLink = () => {
    const text = `Hello Supreme Waste Removal, I would like to check availability for my postcode: ${checkedPostcode || '(Insert Postcode)'}`;
    return `https://wa.me/447940598976?text=${encodeURIComponent(text)}`;
  };

  const handleServiceClick = (serviceId: PageId) => {
    onNavigate(serviceId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-home">
      
      {/* SECTION 1: CINEMATIC HERO */}
      <section className="relative min-h-[92vh] flex items-center justify-center pt-24 pb-16 px-4 sm:px-6 lg:px-8 overflow-hidden border-b border-neutral-900">
        {/* Background Image with Dark Overlay */}
        <div className="absolute inset-0 z-0">
          <img 
            src="https://images.unsplash.com/photo-1549921296-bc643ede1e67?auto=format&fit=crop&w=1440&q=80" 
            alt="Supreme Waste Removal Plymouth Truck Loading" 
            className="w-full h-full object-cover object-center scale-105 animate-pulse duration-[8000ms] opacity-35"
            referrerPolicy="no-referrer"
          />
          <div className="absolute inset-0 bg-neutral-950/85"></div>
          <div className="absolute inset-0 bg-gradient-to-t from-neutral-950 via-transparent to-transparent"></div>
        </div>

        {/* Content Container */}
        <div className="max-w-5xl mx-auto text-center relative z-10 flex flex-col items-center">
          {/* Top Label */}
          <span className="inline-flex items-center space-x-2 text-xs font-bold uppercase tracking-widest text-blue-400 mb-6 bg-blue-500/10 border border-blue-500/20 px-4 py-2 rounded-full">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
            <span>Local waste removal across Plymouth & Devon</span>
          </span>

          {/* Main H1 */}
          <h1 className="text-4xl sm:text-5xl lg:text-7xl font-extrabold tracking-tight text-white uppercase leading-none max-w-4xl">
            Waste Removal <br className="hidden sm:inline" />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-400 to-blue-600">Done Properly.</span>
          </h1>

          {/* Subtext */}
          <p className="mt-6 text-neutral-400 text-base sm:text-xl max-w-3xl leading-relaxed">
            Professional house clearances, rubbish collections, green garden waste, bulky furniture, builders renovation debris, and dependable man-and-van support from a responsive Plymouth team.
          </p>

          {/* CTAs */}
          <div className="mt-10 flex flex-col sm:flex-row gap-4 w-full sm:w-auto">
            <a 
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-blue-600 hover:bg-blue-500 text-white font-extrabold text-base py-4 px-8 rounded-xl transition-all shadow-xl shadow-blue-900/30 flex items-center justify-center group cursor-pointer"
              id="hero-whatsapp-btn"
            >
              <MessageSquare className="w-5 h-5 mr-2" />
              Get a WhatsApp Quote
              <ArrowRight className="w-4 h-4 ml-2 group-hover:translate-x-1 transition-transform" />
            </a>
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-900 hover:bg-neutral-800 text-white border border-neutral-800 hover:border-neutral-700 font-extrabold text-base py-4 px-8 rounded-xl transition-all flex items-center justify-center cursor-pointer"
              id="hero-phone-btn"
            >
              <Phone className="w-5 h-5 mr-2 text-blue-400" />
              Call 07940 598 976
            </a>
          </div>

          {/* Trust points */}
          <div className="mt-12 grid grid-cols-2 sm:grid-cols-3 gap-y-4 gap-x-8 text-xs font-bold uppercase tracking-wider text-neutral-400">
            <div className="flex items-center justify-center space-x-2">
              <Check className="w-4 h-4 text-emerald-500 shrink-0" />
              <span>12 Dedicated Services</span>
            </div>
            <div className="flex items-center justify-center space-x-2">
              <Check className="w-4 h-4 text-emerald-500 shrink-0" />
              <span>Fast Photo Quotes</span>
            </div>
            <div className="hidden sm:flex items-center justify-center space-x-2 col-span-2 sm:col-span-1">
              <Check className="w-4 h-4 text-emerald-500 shrink-0" />
              <span>Local Plymouth Team</span>
            </div>
          </div>
        </div>
      </section>

      {/* SECTION 2: MOVING SERVICE TICKER */}
      <div className="w-full bg-neutral-950 py-4 border-b border-neutral-900 overflow-hidden relative select-none">
        <div className="flex whitespace-nowrap animate-marquee motion-reduce:animate-none space-x-8">
          {Array(3).fill(SERVICES).flat().map((srv, idx) => (
            <span key={idx} className="flex items-center text-xs font-extrabold uppercase tracking-widest text-neutral-400">
              <span className="w-1.5 h-1.5 rounded-full bg-blue-500 mr-3"></span>
              {srv.title.replace(' Plymouth', '')}
            </span>
          ))}
        </div>
      </div>

      {/* SECTION 3: INTRODUCTION */}
      <section className="py-20 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          <div className="lg:col-span-5">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Who We Are</span>
            <h2 className="text-3xl sm:text-4xl lg:text-5xl font-black text-white tracking-tight uppercase leading-none">
              Clear the clutter. <br />
              Reclaim the space.
            </h2>
            <div className="w-12 h-1.5 bg-blue-500 mt-6 rounded-full"></div>
          </div>
          
          <div className="lg:col-span-7 text-neutral-400 space-y-6 text-base sm:text-lg leading-relaxed">
            <p>
              Based at Duncombe Avenue in Plymouth, <strong className="text-white">Supreme Waste Removal Services Ltd</strong> provides prompt, reliable rubbish collection and clearance support for homeowners, tenants, commercial offices, local landlords, and busy tradespeople. We dismantle old structures, lift bulky furniture, clear builder debris, and load everything ourselves.
            </p>
            <p>
              We operate an eco-friendly service where suitable materials are carefully sorted and routed to authorized commercial recycling yards. Avoid the expense and hassle of local skip permits, road space issues, or loading the heavy materials yourself. 
            </p>
            <button 
              onClick={() => onNavigate('services')}
              className="inline-flex items-center text-sm font-bold text-blue-400 hover:text-blue-300 transition-colors group pt-2 cursor-pointer"
              id="intro-explore-services"
            >
              Explore all clearance services
              <ChevronRight className="w-4 h-4 ml-1 group-hover:translate-x-1 transition-transform" />
            </button>
          </div>
        </div>
      </section>

      {/* SECTION 4: ALL SERVICES VISUAL EXPERIENCE (12 services in alternating layout) */}
      <section className="py-20 bg-neutral-900/40 border-t border-b border-neutral-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Section Header */}
          <div className="text-center max-w-3xl mx-auto mb-16">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Our Work Directory</span>
            <h2 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold text-white tracking-tight">
              Twelve Professional Solutions
            </h2>
            <p className="mt-4 text-neutral-400 text-sm sm:text-base">
              Every job has unique requirements. We provide visual layouts and specific details for all of our services, ensuring you find the exact fit for your property or project.
            </p>
          </div>

          {/* Dynamic Editorial Layout List */}
          <div className="space-y-16">
            {SERVICES.map((srv, idx) => {
              const isEven = idx % 2 === 0;
              return (
                <div 
                  key={srv.id}
                  className={`bg-neutral-950/60 border border-neutral-800/80 rounded-2xl overflow-hidden p-6 lg:p-10 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center transition-all duration-300 hover:border-blue-500/40 group`}
                  id={`home-service-editorial-${srv.id}`}
                >
                  {/* Left or Right Image */}
                  <div className={`lg:col-span-6 overflow-hidden rounded-xl h-64 sm:h-80 relative ${isEven ? 'lg:order-1' : 'lg:order-2'}`}>
                    <img 
                      src={srv.imageUrl} 
                      alt={srv.title} 
                      className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105 opacity-80"
                      referrerPolicy="no-referrer"
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-neutral-950/80 via-transparent to-transparent"></div>
                    <span className="absolute top-4 left-4 bg-blue-600 text-white font-extrabold text-sm py-1 px-3 rounded-full shadow-lg">
                      {srv.category}
                    </span>
                  </div>

                  {/* Service Info Content */}
                  <div className={`lg:col-span-6 flex flex-col justify-center ${isEven ? 'lg:order-2' : 'lg:order-1'}`}>
                    <div className="flex items-center space-x-3 text-xs font-bold uppercase text-blue-400 mb-2">
                      <span className="text-neutral-500">Service {srv.number}</span>
                      <span>•</span>
                      <span>Plymouth District</span>
                    </div>
                    
                    <h3 className="text-xl sm:text-2xl font-black text-white uppercase tracking-tight group-hover:text-blue-400 transition-colors">
                      {srv.title}
                    </h3>
                    
                    <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed line-clamp-4">
                      {srv.description}
                    </p>

                    {/* Example List */}
                    <div className="mt-5 grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs font-semibold text-neutral-300">
                      {srv.items.slice(0, 4).map((item, i) => (
                        <div key={i} className="flex items-center space-x-2">
                          <Check className="w-3.5 h-3.5 text-blue-500 shrink-0" />
                          <span>{item}</span>
                        </div>
                      ))}
                    </div>

                    {/* Navigation Buttons */}
                    <div className="mt-6 pt-6 border-t border-neutral-900 flex flex-wrap items-center gap-4">
                      <button
                        onClick={() => handleServiceClick(srv.id)}
                        className="text-xs font-bold text-white uppercase tracking-wider flex items-center hover:text-blue-400 transition-colors cursor-pointer"
                        id={`home-service-link-view-${srv.id}`}
                      >
                        View Service Details
                        <ChevronRight className="w-4 h-4 ml-1" />
                      </button>
                      <span className="text-neutral-800">|</span>
                      <a 
                        href={BUSINESS_INFO.whatsappLink}
                        target="_blank"
                        rel="noreferrer"
                        className="text-xs font-bold text-emerald-400 uppercase tracking-wider flex items-center hover:text-emerald-300 transition-colors cursor-pointer"
                        id={`home-service-link-whatsapp-${srv.id}`}
                      >
                        Get Price for This
                        <MessageSquare className="w-3.5 h-3.5 ml-1" />
                      </a>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* SECTION 5: HOW IT WORKS */}
      <section className="py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Simple Process</span>
          <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-white uppercase">
            Four Steps to a Cleared Space
          </h2>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base">
            Skip-free, stress-free rubbish collection. We handle everything from photos to sweeping up.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 relative">
          {/* Step 1 */}
          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl relative overflow-hidden flex flex-col justify-between min-h-[220px]">
            <div>
              <span className="text-3xl font-black text-blue-500/20 block mb-3">01</span>
              <h3 className="text-lg font-bold text-white mb-2">Send Photos</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Snap a few pictures of the waste and message them over WhatsApp with your collection postcode.
              </p>
            </div>
            <span className="text-[10px] font-bold uppercase tracking-wider text-neutral-500 pt-3 border-t border-neutral-800">
              No home visits required
            </span>
          </div>

          {/* Step 2 */}
          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl relative overflow-hidden flex flex-col justify-between min-h-[220px]">
            <div>
              <span className="text-3xl font-black text-blue-500/20 block mb-3">02</span>
              <h3 className="text-lg font-bold text-white mb-2">Confirm the Plan</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                We send back an estimate. If you approve, we book a morning or afternoon slot that suits you.
              </p>
            </div>
            <span className="text-[10px] font-bold uppercase tracking-wider text-neutral-500 pt-3 border-t border-neutral-800">
              Firm competitive pricing
            </span>
          </div>

          {/* Step 3 */}
          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl relative overflow-hidden flex flex-col justify-between min-h-[220px]">
            <div>
              <span className="text-3xl font-black text-blue-500/20 block mb-3">03</span>
              <h3 className="text-lg font-bold text-white mb-2">Load and Remove</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Our active team arrives in a high-sided waste van. We load the material, ensuring zero mess.
              </p>
            </div>
            <span className="text-[10px] font-bold uppercase tracking-wider text-neutral-500 pt-3 border-t border-neutral-800">
              2-man loading assistance
            </span>
          </div>

          {/* Step 4 */}
          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl relative overflow-hidden flex flex-col justify-between min-h-[220px]">
            <div>
              <span className="text-3xl font-black text-blue-500/20 block mb-3">04</span>
              <h3 className="text-lg font-bold text-white mb-2">Space Cleared</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                We sweep the yard or driveway thoroughly. Payment is handled, and materials go to licensed transfer.
              </p>
            </div>
            <span className="text-[10px] font-bold uppercase tracking-wider text-neutral-500 pt-3 border-t border-neutral-800">
              Responsible local disposal
            </span>
          </div>
        </div>
      </section>

      {/* SECTION 6: HOME AND PROPERTY CLEARANCE FEATURE */}
      <section className="py-20 bg-neutral-900 border-t border-b border-neutral-900/60">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          <div>
            <span className="text-xs font-bold uppercase tracking-widest text-emerald-400 block mb-3">Residential Specialists</span>
            <h2 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-none uppercase">
              Clearance support for homes, rentals & managed properties
            </h2>
            <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed">
              We understand the logistical headaches that come with house sales, probate clearances, or preparing a rental flat for new tenants. Our local Plymouth crew clears bulky items, old carpets, garage shelves, and loft boxes, saving you valuable hours of loading labor.
            </p>
            
            <div className="mt-6 grid grid-cols-2 gap-4 text-xs font-bold uppercase tracking-wide text-neutral-300">
              <div className="flex items-center space-x-2">
                <Check className="w-4 h-4 text-blue-500 shrink-0" />
                <span>Full House Clearances</span>
              </div>
              <div className="flex items-center space-x-2">
                <Check className="w-4 h-4 text-blue-500 shrink-0" />
                <span>End-of-Tenancy Cleans</span>
              </div>
              <div className="flex items-center space-x-2">
                <Check className="w-4 h-4 text-blue-500 shrink-0" />
                <span>Garage & Loft Clears</span>
              </div>
              <div className="flex items-center space-x-2">
                <Check className="w-4 h-4 text-blue-500 shrink-0" />
                <span>Inherited & Probate Estate</span>
              </div>
            </div>

            <div className="mt-8">
              <button 
                onClick={() => onNavigate('house-clearance-plymouth')}
                className="bg-blue-600 hover:bg-blue-500 text-white font-extrabold text-sm py-3 px-6 rounded-xl transition-all inline-flex items-center cursor-pointer"
                id="view-house-clearance-from-home"
              >
                View House Clearance Details
                <ChevronRight className="w-4 h-4 ml-1" />
              </button>
            </div>
          </div>

          <div className="h-96 rounded-2xl overflow-hidden relative border border-neutral-800 shadow-2xl">
            <img 
              src="https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=800&q=80" 
              alt="House Clearance Plymouth Residential Setup" 
              className="w-full h-full object-cover"
              referrerPolicy="no-referrer"
            />
            <div className="absolute inset-0 bg-gradient-to-r from-neutral-900/40 to-transparent"></div>
          </div>
        </div>
      </section>

      {/* SECTION 7: BUILDERS AND COMMERCIAL FEATURE */}
      <section className="py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-gradient-to-br from-neutral-900 to-neutral-950 border border-neutral-800 rounded-2xl p-8 lg:p-12 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center relative overflow-hidden">
          <div className="absolute top-0 left-0 w-80 h-80 bg-blue-500/5 rounded-full blur-3xl"></div>
          
          <div className="lg:col-span-7 relative z-10">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-400 block mb-3">Workplace & Builders Debris</span>
            <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-white tracking-tight leading-tight uppercase">
              Practical collection support for projects & workplaces
            </h2>
            <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed">
              We provide rapid waste logistics for local builders, tradespeople, shopfitters, and commercial operations across Plymouth. Let us clear your construction debris, packaging, pallets, and plasterboard scrap, keeping your site safe and tidy.
            </p>

            <div className="mt-6 flex flex-wrap gap-3">
              <button 
                onClick={() => onNavigate('builders-waste-removal-plymouth')}
                className="bg-neutral-800 hover:bg-neutral-700 text-neutral-300 hover:text-white border border-neutral-700 py-2 px-4 rounded-lg text-xs font-bold transition-all cursor-pointer"
                id="home-builders-btn"
              >
                Builders Waste Removal
              </button>
              <button 
                onClick={() => onNavigate('commercial-waste-removal-plymouth')}
                className="bg-neutral-800 hover:bg-neutral-700 text-neutral-300 hover:text-white border border-neutral-700 py-2 px-4 rounded-lg text-xs font-bold transition-all cursor-pointer"
                id="home-comm-btn"
              >
                Commercial Waste Removal
              </button>
              <button 
                onClick={() => onNavigate('office-clearance-plymouth')}
                className="bg-neutral-800 hover:bg-neutral-700 text-neutral-300 hover:text-white border border-neutral-700 py-2 px-4 rounded-lg text-xs font-bold transition-all cursor-pointer"
                id="home-office-btn"
              >
                Office Clearance
              </button>
            </div>
          </div>

          <div className="lg:col-span-5 h-64 sm:h-80 rounded-xl overflow-hidden relative border border-neutral-800 shadow-xl">
            <img 
              src="https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=800&q=80" 
              alt="Builders Waste Removal Plymouth Site" 
              className="w-full h-full object-cover"
              referrerPolicy="no-referrer"
            />
          </div>
        </div>
      </section>

      {/* SECTION 8: BEFORE AND AFTER */}
      <section className="py-20 bg-neutral-900/20 border-t border-b border-neutral-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Proven Results</span>
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-white uppercase">
              Transformations That Matter
            </h2>
            <p className="mt-3 text-neutral-400 text-sm sm:text-base">
              See the visual standard of our cleared spaces across Plymouth. Slide or view the transitions.
            </p>
          </div>

          {/* Project Selector Tab */}
          <div className="flex justify-center space-x-2 mb-10 overflow-x-auto pb-2">
            {BEFORE_AFTER_STORIES.map((story, idx) => (
              <button
                key={story.id}
                onClick={() => setActiveBaIdx(idx)}
                className={`py-2 px-4 rounded-lg text-xs font-bold uppercase tracking-wider transition-all cursor-pointer whitespace-nowrap ${
                  activeBaIdx === idx 
                    ? 'bg-blue-600 text-white shadow-lg' 
                    : 'bg-neutral-900 text-neutral-400 hover:text-white hover:bg-neutral-800'
                }`}
                id={`ba-tab-${story.id}`}
              >
                {story.category}
              </button>
            ))}
          </div>

          {/* Active Case Study */}
          {(() => {
            const activeStory = BEFORE_AFTER_STORIES[activeBaIdx];
            return (
              <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-center bg-neutral-900 border border-neutral-800 p-6 sm:p-8 rounded-2xl">
                
                {/* Images side-by-side */}
                <div className="lg:col-span-8 grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Before */}
                  <div className="relative rounded-xl overflow-hidden h-64 sm:h-80 border border-neutral-800">
                    <img 
                      src={activeStory.beforeImageUrl} 
                      alt="Before clearance" 
                      className="w-full h-full object-cover"
                      referrerPolicy="no-referrer"
                    />
                    <div className="absolute inset-0 bg-black/35"></div>
                    <span className="absolute bottom-4 left-4 bg-red-600/90 text-white font-extrabold text-xs py-1 px-3 rounded-full uppercase tracking-wider shadow">
                      Before Collection
                    </span>
                  </div>

                  {/* After */}
                  <div className="relative rounded-xl overflow-hidden h-64 sm:h-80 border border-neutral-800">
                    <img 
                      src={activeStory.afterImageUrl} 
                      alt="After clearance" 
                      className="w-full h-full object-cover"
                      referrerPolicy="no-referrer"
                    />
                    <div className="absolute inset-0 bg-black/10"></div>
                    <span className="absolute bottom-4 left-4 bg-emerald-600/90 text-white font-extrabold text-xs py-1 px-3 rounded-full uppercase tracking-wider shadow">
                      Swept After
                    </span>
                  </div>
                </div>

                {/* Case details */}
                <div className="lg:col-span-4 flex flex-col justify-center">
                  <span className="text-xs font-bold text-neutral-500 uppercase tracking-wider block mb-1">
                    Location: {activeStory.location}
                  </span>
                  <h3 className="text-lg font-extrabold text-white uppercase tracking-tight">
                    {activeStory.title}
                  </h3>
                  <p className="mt-3 text-neutral-400 text-xs sm:text-sm leading-relaxed">
                    {activeStory.description}
                  </p>

                  <div className="mt-5 space-y-2 text-xs font-semibold text-neutral-300 border-t border-neutral-800 pt-4">
                    {activeStory.details.map((detail, idx) => (
                      <div key={idx} className="flex items-start space-x-2">
                        <Check className="w-3.5 h-3.5 text-blue-500 shrink-0 mt-0.5" />
                        <span>{detail}</span>
                      </div>
                    ))}
                  </div>

                  <div className="mt-6">
                    <button 
                      onClick={() => onNavigate('before-after')}
                      className="text-xs font-bold text-blue-400 hover:text-blue-300 uppercase tracking-widest inline-flex items-center cursor-pointer"
                      id="view-all-ba-from-home"
                    >
                      View Project gallery →
                    </button>
                  </div>
                </div>
              </div>
            );
          })()}
        </div>
      </section>

      {/* SECTION 9: WHY CHOOSE SUPREME */}
      <section className="py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          <div className="h-80 sm:h-96 rounded-2xl overflow-hidden border border-neutral-800 relative shadow-2xl lg:order-2">
            <img 
              src={vanImg} 
              alt="Supreme Waste Removal Clean Transit Van Loaded" 
              className="w-full h-full object-cover"
              referrerPolicy="no-referrer"
            />
            <div className="absolute inset-0 bg-gradient-to-l from-neutral-950/40 to-transparent"></div>
          </div>

          <div className="lg:order-1">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Our Credentials</span>
            <h2 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-none uppercase mb-6">
              Why Choose Supreme
            </h2>

            <div className="space-y-6">
              {/* Point 1 */}
              <div className="flex items-start">
                <div className="w-8 h-8 rounded-full bg-blue-500/10 border border-blue-500/20 flex items-center justify-center text-blue-400 shrink-0 mr-4 font-bold text-sm">
                  1
                </div>
                <div>
                  <h3 className="text-base font-bold text-white uppercase tracking-tight">Fast photo quotations</h3>
                  <p className="text-xs text-neutral-400 mt-1 leading-relaxed">
                    Simply send clear photos of your waste. We don't drag out quotes with unneeded home visits—you get a reliable price estimate right inside WhatsApp.
                  </p>
                </div>
              </div>

              {/* Point 2 */}
              <div className="flex items-start">
                <div className="w-8 h-8 rounded-full bg-blue-500/10 border border-blue-500/20 flex items-center justify-center text-blue-400 shrink-0 mr-4 font-bold text-sm">
                  2
                </div>
                <div>
                  <h3 className="text-base font-bold text-white uppercase tracking-tight">Flexible collection sizes</h3>
                  <p className="text-xs text-neutral-400 mt-1 leading-relaxed">
                    We handle everything from single bulky sofa pick-ups to massive multi-van probate house clearances. We only charge for the volume of waste we load.
                  </p>
                </div>
              </div>

              {/* Point 3 */}
              <div className="flex items-start">
                <div className="w-8 h-8 rounded-full bg-blue-500/10 border border-blue-500/20 flex items-center justify-center text-blue-400 shrink-0 mr-4 font-bold text-sm">
                  3
                </div>
                <div>
                  <h3 className="text-base font-bold text-white uppercase tracking-tight">Careful lifting and loading</h3>
                  <p className="text-xs text-neutral-400 mt-1 leading-relaxed">
                    Our polite, robust loaders carry heavy items down narrow stairs, navigate doorways without leaving wall marks, and sweep clean the area before departure.
                  </p>
                </div>
              </div>

              {/* Point 4 */}
              <div className="flex items-start">
                <div className="w-8 h-8 rounded-full bg-blue-500/10 border border-blue-500/20 flex items-center justify-center text-blue-400 shrink-0 mr-4 font-bold text-sm">
                  4
                </div>
                <div>
                  <h3 className="text-base font-bold text-white uppercase tracking-tight">Local Plymouth Service</h3>
                  <p className="text-xs text-neutral-400 mt-1 leading-relaxed">
                    Based on Duncombe Avenue, we are an independent Plymouth company. No corporate sub-contractors, just polite local residents committed to service.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* SECTION 10: RESPONSIBLE HANDLING */}
      <section className="py-20 bg-neutral-900 border-t border-b border-neutral-900">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center flex flex-col items-center">
          <Recycle className="w-12 h-12 text-blue-500 mb-6 animate-spin duration-[20000ms]" />
          
          <span className="text-xs font-bold uppercase tracking-widest text-emerald-400 block mb-3">Environmental Standard</span>
          <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-white tracking-tight uppercase leading-tight">
            Responsible sorting & ethical waste routing
          </h2>
          <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed max-w-2xl">
            We prioritize separating timber offcuts, cardboards, scrap metal, and reusable household items to divert waste away from local landfills. By coordinating with certified commercial sorting stations in Plymouth, we ensure maximum material recovery.
          </p>

          <div className="mt-8">
            <button 
              onClick={() => onNavigate('recycling-services-plymouth')}
              className="border border-neutral-700 hover:border-neutral-500 text-neutral-300 hover:text-white font-extrabold text-xs uppercase tracking-widest py-3 px-6 rounded-lg transition-all cursor-pointer"
              id="view-recycling-from-home"
            >
              View Recycling Practices
            </button>
          </div>
        </div>
      </section>

      {/* SECTION 11: AREAS COVERED */}
      <section className="py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-gradient-to-br from-neutral-900 to-neutral-950 border border-neutral-800 rounded-2xl p-8 lg:p-12 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center relative overflow-hidden">
          <div className="lg:col-span-6 relative z-10">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Service Coverage Hub</span>
            <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-white tracking-tight uppercase leading-tight mb-4">
              Plymouth & Surrounding Postcodes
            </h2>
            <p className="text-neutral-400 text-xs sm:text-sm leading-relaxed mb-6">
              Our central base in Duncombe Avenue allows us to dispatch teams quickly across Plymouth, Plympton, Plymstock, Saltash, Torpoint, Ivybridge, and Tavistock. Route availability depends on job scale and location.
            </p>

            {/* List of areas */}
            <div className="grid grid-cols-2 gap-x-4 gap-y-2 text-xs font-bold uppercase tracking-wide text-neutral-300">
              {AREAS_COVERED.map((area, idx) => (
                <div key={idx} className="flex items-center space-x-2">
                  <MapPin className="w-3.5 h-3.5 text-blue-500 shrink-0" />
                  <span>{area.name} <span className="text-neutral-500">({area.postcode.split(', ')[0]})</span></span>
                </div>
              ))}
            </div>
          </div>

          <div className="lg:col-span-6 bg-neutral-950 border border-neutral-800 rounded-xl p-6 relative z-10">
            <span className="text-xs font-extrabold text-neutral-400 uppercase tracking-widest block mb-4">
              Verify Postcode Availability
            </span>
            
            <form onSubmit={handlePostcodeCheck} className="flex flex-col sm:flex-row gap-3">
              <input 
                type="text" 
                placeholder="Insert Postcode (e.g., PL5) *"
                required
                value={checkedPostcode}
                onChange={(e) => setCheckedPostcode(e.target.value)}
                className="flex-1 bg-neutral-900 border border-neutral-800 text-sm rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors uppercase"
              />
              <button 
                type="submit"
                className="bg-blue-600 hover:bg-blue-500 text-white text-xs font-extrabold uppercase tracking-wider py-3 px-6 rounded-lg transition-colors cursor-pointer whitespace-nowrap"
                id="postcode-check-submit"
              >
                Check Postcode
              </button>
            </form>

            {postcodeStatus && (
              <div className="mt-4 p-3 bg-neutral-900/60 border border-neutral-850 rounded-lg text-xs leading-relaxed">
                <p className="text-neutral-300 font-medium mb-3">{postcodeStatus}</p>
                <a 
                  href={getPostcodeWhatsAppLink()}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center text-xs font-bold text-emerald-400 hover:text-emerald-300 uppercase tracking-wider cursor-pointer"
                  id="postcode-whatsapp-confirm"
                >
                  Confirm Availability on WhatsApp →
                </a>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* SECTION 12: FAQs */}
      <section className="py-20 bg-neutral-900/40 border-t border-b border-neutral-900">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Got Questions?</span>
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-white uppercase">
              Frequently Asked Questions
            </h2>
            <p className="mt-3 text-neutral-400 text-sm sm:text-base">
              Clear answers regarding quotes, bulky collections, skip hire comparisons, and flat clearances.
            </p>
          </div>

          <div className="space-y-4" id="home-faqs-accordion">
            {GENERAL_FAQS.map((faq, idx) => {
              const isOpen = openFaqIndex === idx;
              return (
                <div 
                  key={faq.id}
                  className="bg-neutral-950 border border-neutral-800 rounded-xl overflow-hidden transition-all duration-300"
                >
                  <button
                    onClick={() => setOpenFaqIndex(isOpen ? null : idx)}
                    className="w-full flex justify-between items-center text-left p-5 text-sm sm:text-base font-extrabold text-white hover:text-blue-400 transition-colors focus:outline-none"
                    aria-expanded={isOpen}
                    id={`faq-btn-${faq.id}`}
                  >
                    <span>{faq.question}</span>
                    <ChevronDown className={`w-4 h-4 text-neutral-500 shrink-0 ml-4 transition-transform duration-250 ${isOpen ? 'rotate-180 text-blue-500' : 'rotate-0'}`} />
                  </button>
                  
                  {isOpen && (
                    <div className="px-5 pb-5 pt-1 border-t border-neutral-900 text-xs sm:text-sm text-neutral-400 leading-relaxed animate-in fade-in duration-200">
                      {faq.answer}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* SECTION 13: FINAL CALL TO ACTION */}
      <section className="relative py-24 px-4 sm:px-6 lg:px-8 border-b border-neutral-900 overflow-hidden flex items-center justify-center">
        <div className="absolute inset-0 z-0">
          <img 
            src="https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=1200&q=80" 
            alt="Supreme Waste Removal Cargo" 
            className="w-full h-full object-cover object-center opacity-25"
            referrerPolicy="no-referrer"
          />
          <div className="absolute inset-0 bg-neutral-950/85"></div>
        </div>

        <div className="max-w-3xl mx-auto text-center relative z-10 flex flex-col items-center">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-400 mb-3 block">Ready to Clear It?</span>
          <h2 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold tracking-tight text-white uppercase leading-none mb-4">
            Send the Photos. We Handle the Heavy Lifting.
          </h2>
          <p className="text-neutral-400 text-sm sm:text-base leading-relaxed mb-8 max-w-xl">
            Share the collection postcode, clear images, and your preferred collection date for an instant response from Supreme Waste Removal.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 w-full sm:w-auto">
            <a 
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold text-sm py-4 px-8 rounded-xl transition-all shadow-lg shadow-emerald-950/20 flex items-center justify-center cursor-pointer"
              id="final-cta-whatsapp"
            >
              <MessageSquare className="w-5 h-5 mr-2" />
              Start WhatsApp Quote
            </a>
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-900 hover:bg-neutral-800 text-white border border-neutral-800 hover:border-neutral-700 font-extrabold text-sm py-4 px-8 rounded-xl transition-all flex items-center justify-center cursor-pointer"
              id="final-cta-phone"
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
