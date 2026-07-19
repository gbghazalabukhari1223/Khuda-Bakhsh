/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React from 'react';
import { PageId } from '../types';
import { BUSINESS_INFO } from '../data';
import { ShieldCheck, Truck, Recycle, Award, Phone, MessageSquare } from 'lucide-react';
import vanImg from '../assets/van.jpg';

interface AboutProps {
  onNavigate: (pageId: PageId) => void;
}

export default function About({ onNavigate }: AboutProps) {
  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Editorial Intro Banner */}
      <section className="relative py-24 bg-neutral-900 border-b border-neutral-850 overflow-hidden">
        <div className="absolute inset-0 opacity-25">
          <img 
            src={vanImg} 
            alt="Supreme Waste Removal Transit Van" 
            className="w-full h-full object-cover object-center"
            referrerPolicy="no-referrer"
          />
        </div>
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center relative z-10 flex flex-col items-center">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-400 mb-3 bg-blue-500/10 px-3 py-1 rounded-full">
            Our Plymouth Roots
          </span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white tracking-tight leading-tight uppercase">
            About Supreme Waste Removal
          </h1>
          <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed max-w-2xl">
            Established at Duncombe Avenue in Plymouth, we are an independent team of waste logistics specialists offering clean, efficient property clearances and man-and-van support.
          </p>
        </div>
      </section>

      {/* Local Plymouth Identity & Approach */}
      <section className="py-20 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
        <div>
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-2">Our Business Ethics</span>
          <h2 className="text-2xl sm:text-3xl font-black text-white uppercase tracking-tight mb-6">
            A reliable, hands-on team you can trust
          </h2>
          <div className="text-neutral-400 text-sm sm:text-base space-y-4 leading-relaxed">
            <p>
              At Supreme Waste Removal Services Ltd, we believe that rubbish collection shouldn't be a messy, stressful ordeal. Unlike typical national skip broker networks, we are a friendly local team. When you hire us, we handle all the manual lifting, carrying, and loading ourselves.
            </p>
            <p>
              We focus on speed and transparency. By requesting photo submissions over WhatsApp, we provide competitive, volume-based estimates in minutes, preventing any unexpected surprises on site. We take extreme pride in keeping Plymouth residential areas and commercial workspaces safe and tidy.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl">
            <ShieldCheck className="w-8 h-8 text-blue-500 mb-3" />
            <h3 className="text-sm font-bold text-white uppercase tracking-wider mb-2">Fully Insured</h3>
            <p className="text-xs text-neutral-400 leading-relaxed">
              Complete public liability coverage ensures total protection for your property interior during heavy maneuvers.
            </p>
          </div>

          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl">
            <Truck className="w-8 h-8 text-blue-500 mb-3" />
            <h3 className="text-sm font-bold text-white uppercase tracking-wider mb-2">Clean High-Side Vans</h3>
            <p className="text-xs text-neutral-400 leading-relaxed">
              Modern, transit-style cargo vehicles equipped with straps and tools for secure, scratch-free transport.
            </p>
          </div>

          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl">
            <Recycle className="w-8 h-8 text-blue-500 mb-3" />
            <h3 className="text-sm font-bold text-white uppercase tracking-wider mb-2">Recycling Priority</h3>
            <p className="text-xs text-neutral-400 leading-relaxed">
              We separate cardboards, scrap metal, and woods, routing suitable material to licensed commercial sorting points.
            </p>
          </div>

          <div className="bg-neutral-900 border border-neutral-800 p-6 rounded-xl">
            <Award className="w-8 h-8 text-blue-500 mb-3" />
            <h3 className="text-sm font-bold text-white uppercase tracking-wider mb-2">Swept-Clean Finish</h3>
            <p className="text-xs text-neutral-400 leading-relaxed">
              We don't leave mess on your driveway. Our crew rakes and sweeps clean every yard or building site before departure.
            </p>
          </div>
        </div>
      </section>

      {/* Who the Services Support */}
      <section className="py-20 bg-neutral-900/40 border-t border-b border-neutral-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center max-w-2xl mx-auto mb-16">
            <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-2">Our Clientele</span>
            <h2 className="text-3xl font-extrabold text-white uppercase">Who We Help</h2>
            <p className="mt-2 text-neutral-400 text-xs sm:text-sm">
              We deliver customized clearance solutions tailored to four core user groups across Plymouth.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div className="bg-neutral-950 border border-neutral-800 p-6 rounded-xl text-center">
              <span className="text-2xl font-black text-blue-500 block mb-3">01</span>
              <h3 className="text-sm font-bold text-white uppercase tracking-wide mb-2">Homeowners</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Reclaiming overgrown gardens, crowded garages, attics, or clearing rooms after DIY kitchen remodeling.
              </p>
            </div>

            <div className="bg-neutral-950 border border-neutral-800 p-6 rounded-xl text-center">
              <span className="text-2xl font-black text-blue-500 block mb-3">02</span>
              <h3 className="text-sm font-bold text-white uppercase tracking-wide mb-2">Landlords</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Clearing abandoned items or general household rubbish between rental tenancies to guarantee quick re-letting.
              </p>
            </div>

            <div className="bg-neutral-950 border border-neutral-800 p-6 rounded-xl text-center">
              <span className="text-2xl font-black text-blue-500 block mb-3">03</span>
              <h3 className="text-sm font-bold text-white uppercase tracking-wide mb-2">Tradespeople</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Assisting builders, carpenters, and plumbers with same-day collection of renovation debris, timber, and plasterboard.
              </p>
            </div>

            <div className="bg-neutral-950 border border-neutral-800 p-6 rounded-xl text-center">
              <span className="text-2xl font-black text-blue-500 block mb-3">04</span>
              <h3 className="text-sm font-bold text-white uppercase tracking-wide mb-2">Local Offices</h3>
              <p className="text-xs text-neutral-400 leading-relaxed">
                Swift clearance of office desks, filing cabinets, partitioned panels, cardboard archives, and electrical appliances.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Operating Principles & Standards */}
      <section className="py-20 max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 className="text-2xl sm:text-3xl font-black text-white uppercase tracking-tight text-center mb-12">
          Collection & Service Standards
        </h2>

        <div className="space-y-6">
          <div className="p-6 bg-neutral-900 rounded-xl border border-neutral-800/60">
            <h3 className="text-sm font-bold text-blue-400 uppercase tracking-wide mb-2">1. Punctuality & Communication</h3>
            <p className="text-xs sm:text-sm text-neutral-400 leading-relaxed">
              We respect your schedule. Our team provides realistic arrival windows and calls you when we are 20 minutes away, ensuring you never sit waiting around.
            </p>
          </div>

          <div className="p-6 bg-neutral-900 rounded-xl border border-neutral-800/60">
            <h3 className="text-sm font-bold text-blue-400 uppercase tracking-wide mb-2">2. Respect for Property Interiors</h3>
            <p className="text-xs sm:text-sm text-neutral-400 leading-relaxed">
              Carrying bulky items through tight terraced corridors can be risky. Our loaders disassemble oversized wardrobes, lift sofas vertically, and use protective techniques to prevent wall scuffs and doorway chips.
            </p>
          </div>

          <div className="p-6 bg-neutral-900 rounded-xl border border-neutral-800/60">
            <h3 className="text-sm font-bold text-blue-400 uppercase tracking-wide mb-2">3. Accurate Volume Tracking</h3>
            <p className="text-xs sm:text-sm text-neutral-400 leading-relaxed">
              We charge transparently based on the actual volume filled in our trucks. If your pile takes up less space than estimated from photographs, we adjust our price on site. No unfair fixed-rate surcharges.
            </p>
          </div>
        </div>
      </section>

      {/* Final Call to Action */}
      <section className="bg-neutral-900 py-16 px-4 sm:px-6 lg:px-8 text-center border-t border-neutral-800">
        <div className="max-w-2xl mx-auto">
          <h2 className="text-2xl sm:text-3xl font-extrabold text-white uppercase leading-none mb-4">
            Request a Quick Quote Today
          </h2>
          <p className="text-neutral-400 text-xs sm:text-sm leading-relaxed mb-6">
            Contact us via telephone or submit clear photographs on WhatsApp to book your clearance slot.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a 
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-3 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer"
            >
              <MessageSquare className="w-4 h-4 mr-2" />
              WhatsApp Quote
            </a>
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-800 hover:bg-neutral-700 text-white border border-neutral-700 py-3 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer"
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
