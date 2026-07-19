/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId } from '../types';
import { BUSINESS_INFO, SERVICES } from '../data';
import logoImg from '../assets/logo.jpg';
import { 
  Facebook, 
  Mail, 
  Phone, 
  MapPin, 
  ChevronDown, 
  Send, 
  ExternalLink,
  MessageSquare
} from 'lucide-react';

interface FooterProps {
  onNavigate: (pageId: PageId) => void;
}

export default function Footer({ onNavigate }: FooterProps) {
  const [activeGroup, setActiveGroup] = useState<string | null>(null);
  
  // States for the compact WhatsApp Enquiry Form
  const [formName, setFormName] = useState('');
  const [formPostcode, setFormPostcode] = useState('');
  const [formMessage, setFormMessage] = useState('');

  const toggleGroup = (group: string) => {
    if (activeGroup === group) {
      setActiveGroup(null);
    } else {
      setActiveGroup(group);
    }
  };

  const handleLinkClick = (pageId: PageId) => {
    onNavigate(pageId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Prepares the WhatsApp text with user details from the form
  const handleQuickEnquiry = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formName || !formPostcode) {
      alert("Please fill in your name and postcode to continue.");
      return;
    }
    const text = `Hello Supreme Waste Removal, I would like a quick quote.\n\n` +
                 `Name: ${formName}\n` +
                 `Postcode: ${formPostcode}\n` +
                 `Details: ${formMessage || 'General clearance request'}`;
    const encodedText = encodeURIComponent(text);
    const waUrl = `https://wa.me/447940598976?text=${encodedText}`;
    window.open(waUrl, '_blank');
  };

  return (
    <footer className="w-full bg-neutral-950 text-neutral-300 border-t border-neutral-900 pt-16 pb-20 md:pb-8 swr-global-footer relative z-10">
      
      {/* 1. LARGE PRE-FOOTER CALL TO ACTION */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mb-16">
        <div className="bg-gradient-to-br from-neutral-900 to-neutral-950 border border-neutral-800 p-8 md:p-12 rounded-2xl flex flex-col md:flex-row items-center justify-between gap-8 shadow-2xl relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-96 h-96 bg-blue-500/5 rounded-full blur-3xl group-hover:bg-blue-500/10 transition-colors duration-500"></div>
          
          <div className="max-w-2xl relative z-10 text-center md:text-left">
            <span className="inline-flex items-center text-xs font-bold uppercase tracking-widest text-blue-400 mb-3 bg-blue-500/10 px-2.5 py-1 rounded-full">
              Free Photo Quotations
            </span>
            <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-white tracking-tight leading-tight">
              Need unwanted waste cleared?
            </h2>
            <p className="mt-4 text-neutral-400 text-sm sm:text-base leading-relaxed">
              Send photographs, your collection postcode, and a short description for a quick, no-obligation quotation from the Supreme Waste Removal team.
            </p>
          </div>

          <div className="flex flex-col sm:flex-row gap-4 w-full md:w-auto shrink-0 relative z-10">
            <a
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-center py-4 px-6 rounded-xl text-sm flex items-center justify-center transition-all shadow-lg shadow-emerald-950/20"
              id="footer-cta-whatsapp"
            >
              <MessageSquare className="w-5 h-5 mr-2" />
              WhatsApp Quote
            </a>
            <a
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-800 hover:bg-neutral-700 text-white font-bold text-center py-4 px-6 rounded-xl text-sm flex items-center justify-center transition-all border border-neutral-700"
              id="footer-cta-call"
            >
              <Phone className="w-5 h-5 mr-2 text-blue-500" />
              Call 07940 598 976
            </a>
          </div>
        </div>
      </div>

      {/* 2. MAIN FOOTER CONTENT GRID */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-12 border-b border-neutral-900 pb-12">
        
        {/* Column 1: Company Info */}
        <div className="flex flex-col space-y-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-white rounded-lg p-0.5 shadow-md flex items-center justify-center overflow-hidden shrink-0 border border-neutral-800">
              <img 
                src={logoImg} 
                alt="Supreme Waste Removal Logo" 
                className="w-full h-full object-contain"
                referrerPolicy="no-referrer"
              />
            </div>
            <div className="flex flex-col">
              <span className="font-black text-xl sm:text-2xl tracking-tight text-white leading-none">
                SUPREME<span className="text-blue-500 font-light ml-1">WASTE</span>
              </span>
              <span className="text-[10px] tracking-widest text-neutral-500 uppercase font-bold mt-1">
                Removal Services Ltd
              </span>
            </div>
          </div>
          
          <p className="text-sm text-neutral-400 leading-relaxed pt-2">
            Professional waste clearance, garden rubbish collection, and man-and-van support based in Plymouth, United Kingdom. Proudly handling your materials with maximum recycling priority.
          </p>

          <div className="flex flex-col space-y-2 text-xs text-neutral-400 pt-1">
            <span className="flex items-center">✓ 2-Man Loading Assistance</span>
            <span className="flex items-center">✓ No Skip Permits Required</span>
            <span className="flex items-center">✓ Volume-Based Pricing Model</span>
          </div>

          <div className="flex space-x-3 pt-2">
            <a 
              href={BUSINESS_INFO.facebook} 
              target="_blank" 
              rel="noreferrer"
              className="w-8 h-8 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center hover:bg-blue-600 hover:text-white transition-colors"
              aria-label="Facebook Profile"
              id="footer-social-facebook"
            >
              <Facebook className="w-4 h-4" />
            </a>
            <a 
              href={`mailto:${BUSINESS_INFO.email}`} 
              className="w-8 h-8 rounded-full bg-neutral-900 border border-neutral-800 flex items-center justify-center hover:bg-blue-500 hover:text-white transition-colors"
              aria-label="Email Support"
              id="footer-social-email"
            >
              <Mail className="w-4 h-4" />
            </a>
          </div>
        </div>

        {/* Column 2: Quick Links (with collapsible mobile logic) */}
        <div>
          {/* Mobile Accordion Header */}
          <button 
            onClick={() => toggleGroup('quick')}
            className="w-full flex justify-between items-center md:block text-left border-b border-neutral-900 pb-2 md:border-0 md:pb-0 focus:outline-none"
            id="footer-col-quick-header"
          >
            <h3 className="text-sm font-extrabold uppercase tracking-widest text-white">
              Website Directory
            </h3>
            <ChevronDown className={`w-4 h-4 text-neutral-500 md:hidden transition-transform ${activeGroup === 'quick' ? 'rotate-180' : 'rotate-0'}`} />
          </button>
          
          {/* Link Body */}
          <div className={`mt-4 flex flex-col space-y-2.5 text-sm ${activeGroup === 'quick' ? 'block' : 'hidden md:flex'}`}>
            <button onClick={() => handleLinkClick('home')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">Home Page</button>
            <button onClick={() => handleLinkClick('about-us')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">About Our Business</button>
            <button onClick={() => handleLinkClick('services')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">All Clearance Services</button>
            <button onClick={() => handleLinkClick('gallery')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">Gallery Portfolio</button>
            <button onClick={() => handleLinkClick('before-after')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">Before & After Projects</button>
            <button onClick={() => handleLinkClick('areas-covered')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">Service Areas Covered</button>
            <button onClick={() => handleLinkClick('faqs')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">FAQs Accordion</button>
            <button onClick={() => handleLinkClick('contact')} className="text-left text-neutral-400 hover:text-blue-400 transition-colors">Contact Our Office</button>
            <button onClick={() => handleLinkClick('sitemap')} className="text-left text-neutral-500 hover:text-blue-400 transition-colors text-xs">XML Sitemap</button>
          </div>
        </div>

        {/* Column 3: Clearance Services List */}
        <div>
          {/* Mobile Accordion Header */}
          <button 
            onClick={() => toggleGroup('services')}
            className="w-full flex justify-between items-center md:block text-left border-b border-neutral-900 pb-2 md:border-0 md:pb-0 focus:outline-none"
            id="footer-col-services-header"
          >
            <h3 className="text-sm font-extrabold uppercase tracking-widest text-white">
              Clearance Services
            </h3>
            <ChevronDown className={`w-4 h-4 text-neutral-500 md:hidden transition-transform ${activeGroup === 'services' ? 'rotate-180' : 'rotate-0'}`} />
          </button>
          
          {/* Link Body */}
          <div className={`mt-4 flex flex-col space-y-2.5 text-sm ${activeGroup === 'services' ? 'block' : 'hidden md:flex'}`}>
            {SERVICES.map(srv => (
              <button 
                key={srv.id} 
                onClick={() => handleLinkClick(srv.id)} 
                className="text-left text-neutral-400 hover:text-blue-400 transition-colors text-xs"
                id={`footer-link-service-${srv.id}`}
              >
                {srv.title.replace(' Plymouth', '')}
              </button>
            ))}
          </div>
        </div>

        {/* Column 4: Contact & Compact WhatsApp Form */}
        <div>
          <h3 className="text-sm font-extrabold uppercase tracking-widest text-white mb-4">
            Contact & Enquiries
          </h3>
          
          <div className="flex flex-col space-y-3 text-sm text-neutral-400 mb-6">
            <a href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`} className="flex items-start hover:text-blue-400 transition-colors">
              <Phone className="w-4 h-4 mr-2 text-blue-500 shrink-0 mt-0.5" />
              <span>07940 598 976</span>
            </a>
            <a href={`mailto:${BUSINESS_INFO.email}`} className="flex items-start hover:text-blue-400 transition-colors">
              <Mail className="w-4 h-4 mr-2 text-blue-500 shrink-0 mt-0.5" />
              <span className="break-all">{BUSINESS_INFO.email}</span>
            </a>
            <div className="flex items-start">
              <MapPin className="w-4 h-4 mr-2 text-blue-500 shrink-0 mt-0.5" />
              <span className="text-xs leading-relaxed">
                53 Duncombe Avenue, Plymouth, PL5 2JT, UK
              </span>
            </div>
          </div>

          {/* COMPACT WHATSAPP FORM */}
          <form onSubmit={handleQuickEnquiry} className="bg-neutral-900 border border-neutral-800/80 rounded-xl p-4 flex flex-col space-y-2.5" id="footer-whatsapp-quick-form">
            <span className="text-[11px] font-bold text-neutral-400 uppercase tracking-wider block">
              Quick WhatsApp Enquiry
            </span>
            <input 
              type="text" 
              placeholder="Your Name *"
              required
              value={formName}
              onChange={(e) => setFormName(e.target.value)}
              className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
            />
            <input 
              type="text" 
              placeholder="Plymouth Postcode (e.g. PL5) *"
              required
              value={formPostcode}
              onChange={(e) => setFormPostcode(e.target.value)}
              className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
            />
            <textarea 
              placeholder="What do you need cleared?"
              rows={2}
              value={formMessage}
              onChange={(e) => setFormMessage(e.target.value)}
              className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors resize-none"
            />
            <button
              type="submit"
              className="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs py-2 rounded-lg transition-colors flex items-center justify-center cursor-pointer"
            >
              Enquire on WhatsApp
              <ExternalLink className="w-3 h-3 ml-1" />
            </button>
          </form>
        </div>
      </div>

      {/* 3. LEGAL ACCREDITATION & TRADEMARK LINE */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col md:flex-row items-center justify-between text-xs text-neutral-500 space-y-4 md:space-y-0">
        <div>
          © {new Date().getFullYear()} Supreme Waste Removal Services Ltd. All rights reserved. Registered in the United Kingdom.
        </div>
        <div className="flex flex-wrap gap-4 justify-center md:justify-end">
          <button onClick={() => handleLinkClick('privacy-policy')} className="hover:text-blue-400 transition-colors">Privacy Policy</button>
          <span>•</span>
          <button onClick={() => handleLinkClick('terms-and-conditions')} className="hover:text-blue-400 transition-colors">Terms & Conditions</button>
          <span>•</span>
          <button onClick={() => handleLinkClick('cookie-policy')} className="hover:text-blue-400 transition-colors">Cookie Policy</button>
        </div>
      </div>
    </footer>
  );
}
