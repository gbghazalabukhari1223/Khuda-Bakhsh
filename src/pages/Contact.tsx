/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId, ContactFormData } from '../types';
import { BUSINESS_INFO, SERVICES } from '../data';
import { Mail, Phone, MapPin, Facebook, MessageSquare, Send, CheckCircle2, Camera, Info } from 'lucide-react';

interface ContactProps {
  onNavigate: (pageId: PageId) => void;
}

export default function Contact({ onNavigate }: ContactProps) {
  const [formData, setFormData] = useState<ContactFormData>({
    name: '',
    phone: '',
    email: '',
    postcode: '',
    service: 'house-clearance-plymouth',
    description: '',
    preferredDate: '',
    consent: false
  });

  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name || !formData.phone || !formData.postcode || !formData.consent) {
      alert("Please fill in all required fields and accept the consent checkbox.");
      return;
    }
    
    setIsSubmitted(true);
  };

  // Helper to generate dynamic WhatsApp text pre-filled with form details
  const getDynamicWhatsAppText = () => {
    const selectedService = SERVICES.find(s => s.id === formData.service)?.title || formData.service;
    const text = `Hello Supreme Waste Removal, I have submitted an intake form.\n\n` +
                 `Name: ${formData.name}\n` +
                 `Phone: ${formData.phone}\n` +
                 `Email: ${formData.email || 'N/A'}\n` +
                 `Postcode: ${formData.postcode}\n` +
                 `Service Required: ${selectedService}\n` +
                 `Message: ${formData.description || 'General clearance request'}\n` +
                 `Preferred Date: ${formData.preferredDate || 'Flexible'}`;
    return `https://wa.me/447940598976?text=${encodeURIComponent(text)}`;
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Office Communication</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Contact Plymouth Crew
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Book a collection or request an estimate. Submit details below or coordinate directly via phone.
          </p>
        </div>
      </section>

      {/* Main Grid Content */}
      <section className="py-16 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 lg:grid-cols-12 gap-12">
        
        {/* Left Column (Contact details, Map and upload advice) */}
        <div className="lg:col-span-5 space-y-8">
          <div>
            <h2 className="text-xl font-black text-white uppercase tracking-tight mb-4 border-b border-neutral-900 pb-2">
              Plymouth Head Office
            </h2>
            <p className="text-neutral-400 text-xs sm:text-sm leading-relaxed mb-6">
              Our service desk responds immediately to messages, emails, and phone enquiries. You can also visit or send mail to our Duncombe Avenue headquarters.
            </p>

            <div className="space-y-4 text-xs sm:text-sm text-neutral-300">
              <a href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`} className="p-4 bg-neutral-900 rounded-xl border border-neutral-850 flex items-start hover:border-blue-500/30 transition-all">
                <Phone className="w-5 h-5 text-blue-500 shrink-0 mr-3 mt-0.5" />
                <div>
                  <h3 className="font-bold text-white uppercase tracking-wider">Call Our Team</h3>
                  <p className="text-neutral-400 mt-1">07940 598 976 (Direct Line)</p>
                  <p className="text-neutral-500 text-[11px] mt-0.5">Response Time: Immediate</p>
                </div>
              </a>

              <a href={`mailto:${BUSINESS_INFO.email}`} className="p-4 bg-neutral-900 rounded-xl border border-neutral-850 flex items-start hover:border-blue-500/30 transition-all">
                <Mail className="w-5 h-5 text-blue-500 shrink-0 mr-3 mt-0.5" />
                <div>
                  <h3 className="font-bold text-white uppercase tracking-wider">Email Support</h3>
                  <p className="text-neutral-400 mt-1 break-all">{BUSINESS_INFO.email}</p>
                  <p className="text-neutral-500 text-[11px] mt-0.5">Response Time: Under 2 Hours</p>
                </div>
              </a>

              <div className="p-4 bg-neutral-900 rounded-xl border border-neutral-850 flex items-start">
                <MapPin className="w-5 h-5 text-blue-500 shrink-0 mr-3 mt-0.5" />
                <div>
                  <h3 className="font-bold text-white uppercase tracking-wider">Headquarters</h3>
                  <p className="text-neutral-400 mt-1">53 Duncombe Avenue, Plymouth, PL5 2JT, UK</p>
                  <p className="text-neutral-500 text-[11px] mt-0.5">Central Hub Dispatch</p>
                </div>
              </div>
            </div>
          </div>

          {/* Upload Advice Alert */}
          <div className="p-5 bg-neutral-900/60 border border-neutral-800 rounded-xl flex items-start space-x-3.5">
            <Camera className="w-5 h-5 text-blue-500 shrink-0 mt-1" />
            <div>
              <h4 className="text-xs font-bold text-white uppercase tracking-wider mb-1">Photograph Submissions</h4>
              <p className="text-xs text-neutral-400 leading-relaxed">
                As direct file upload is restricted by sandboxed browser frame layers, we kindly advise customers to <strong>continue on WhatsApp</strong> to send photos of their pile. This allows our team to estimate your job within minutes.
              </p>
            </div>
          </div>

          {/* Social connections */}
          <div className="flex items-center space-x-3 text-xs font-bold uppercase text-neutral-400 pt-2">
            <span>Follow Us:</span>
            <a 
              href={BUSINESS_INFO.facebook} 
              target="_blank" 
              rel="noreferrer"
              className="inline-flex items-center text-blue-400 hover:text-blue-300"
              id="contact-facebook"
            >
              <Facebook className="w-4 h-4 mr-1" />
              Facebook Profile
            </a>
          </div>

          {/* Clean Map Placeholder */}
          <div className="bg-neutral-900 border border-neutral-850 rounded-xl h-48 overflow-hidden relative shadow-lg">
            <div className="absolute inset-0 bg-neutral-950/20 z-10 pointer-events-none"></div>
            {/* Visual static Map style representation */}
            <div className="w-full h-full bg-neutral-950 flex flex-col items-center justify-center text-center p-4">
              <MapPin className="w-8 h-8 text-blue-500 mb-2 animate-bounce" />
              <span className="text-xs font-bold text-white uppercase">Plymouth PL5 2JT Hub</span>
              <span className="text-[10px] text-neutral-500 mt-1">Central coverage dispatched across Devon</span>
              <a 
                href="https://maps.google.com/?q=53+Duncombe+Avenue+Plymouth+PL5+2JT"
                target="_blank"
                rel="noreferrer"
                className="mt-3 text-[10px] text-blue-400 hover:text-blue-300 underline font-bold uppercase tracking-wider cursor-pointer"
                id="contact-map-link"
              >
                Open Google Maps Location
              </a>
            </div>
          </div>
        </div>

        {/* Right Column (Contact Intake Form / Success Portal) */}
        <div className="lg:col-span-7">
          {isSubmitted ? (
            <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-8 text-center flex flex-col items-center justify-center min-h-[450px] animate-in fade-in duration-200" id="contact-success-portal">
              <CheckCircle2 className="w-16 h-16 text-emerald-500 mb-6" />
              
              <h2 className="text-2xl font-black text-white uppercase tracking-tight mb-2">
                Enquiry Received!
              </h2>
              <p className="text-neutral-400 text-xs sm:text-sm max-w-md leading-relaxed mb-8">
                Thank you, {formData.name}. Your details have been logged with our Plymouth desk. To speed up your quote and send pictures of your clutter, please tap below to continue on WhatsApp.
              </p>

              <div className="flex flex-col sm:flex-row gap-4 w-full justify-center">
                <a
                  href={getDynamicWhatsAppText()}
                  target="_blank"
                  rel="noreferrer"
                  className="bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold py-3.5 px-6 rounded-xl text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer shadow-lg shadow-emerald-950/20"
                  id="success-send-wa"
                >
                  <MessageSquare className="w-4 h-4 mr-2" />
                  Continue on WhatsApp (Fastest)
                </a>
                <button
                  onClick={() => {
                    setIsSubmitted(false);
                    setFormData({
                      name: '',
                      phone: '',
                      email: '',
                      postcode: '',
                      service: 'house-clearance-plymouth',
                      description: '',
                      preferredDate: '',
                      consent: false
                    });
                  }}
                  className="bg-neutral-850 hover:bg-neutral-800 border border-neutral-700 text-white font-bold py-3.5 px-6 rounded-xl text-xs uppercase tracking-wider cursor-pointer"
                  id="success-reset-btn"
                >
                  Submit Another Enquiry
                </button>
              </div>
            </div>
          ) : (
            <div className="bg-neutral-900 border border-neutral-800 rounded-2xl p-6 sm:p-8">
              <span className="text-[10px] font-extrabold text-blue-400 uppercase tracking-widest block mb-1">
                Intake Desk
              </span>
              <h2 className="text-xl font-black text-white uppercase tracking-tight mb-6">
                Clearance Booking Form
              </h2>

              <form onSubmit={handleSubmit} className="space-y-4 text-xs" id="contact-intake-form">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Name */}
                  <div>
                    <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                      Your Name *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder="e.g. John Doe"
                      className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
                    />
                  </div>

                  {/* Phone */}
                  <div>
                    <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                      Phone Number *
                    </label>
                    <input
                      type="tel"
                      required
                      value={formData.phone}
                      onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                      placeholder="e.g. 07940 598 976"
                      className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Email */}
                  <div>
                    <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                      Email Address
                    </label>
                    <input
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      placeholder="e.g. john@example.com"
                      className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
                    />
                  </div>

                  {/* Postcode */}
                  <div>
                    <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                      Collection Postcode *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.postcode}
                      onChange={(e) => setFormData({ ...formData, postcode: e.target.value })}
                      placeholder="e.g. PL5 2JT"
                      className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors uppercase"
                    />
                  </div>
                </div>

                {/* Service Dropdown */}
                <div>
                  <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                    Service Required *
                  </label>
                  <select
                    value={formData.service}
                    onChange={(e) => setFormData({ ...formData, service: e.target.value as any })}
                    className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
                  >
                    {SERVICES.map(srv => (
                      <option key={srv.id} value={srv.id}>
                        {srv.title}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Preferred Date */}
                <div>
                  <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                    Preferred Collection Date
                  </label>
                  <input
                    type="date"
                    value={formData.preferredDate}
                    onChange={(e) => setFormData({ ...formData, preferredDate: e.target.value })}
                    className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors"
                  />
                </div>

                {/* Description */}
                <div>
                  <label className="block text-[11px] font-bold text-neutral-400 uppercase tracking-wider mb-1.5">
                    Clutter Description & Details
                  </label>
                  <textarea
                    rows={4}
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    placeholder="e.g. Single bulky green fabric sofa, old broken fridge in the garden path, three boxes of general rubbish..."
                    className="w-full bg-neutral-950 border border-neutral-800 text-xs rounded-lg px-4 py-3 text-white focus:outline-none focus:border-blue-500 transition-colors resize-none"
                  />
                </div>

                {/* Consent checkbox */}
                <div className="flex items-start space-x-2 pt-2">
                  <input
                    type="checkbox"
                    required
                    id="consent-check"
                    checked={formData.consent}
                    onChange={(e) => setFormData({ ...formData, consent: e.target.checked })}
                    className="mt-0.5 rounded text-blue-600 focus:ring-blue-500 border-neutral-800 bg-neutral-950 h-4 w-4"
                  />
                  <label htmlFor="consent-check" className="text-[10px] text-neutral-400 leading-relaxed cursor-pointer selection:bg-transparent">
                    By submitting this intake form, I grant consent to Supreme Waste Removal Services Ltd to process my postcode and contact information to generate estimates and coordinate route schedules. *
                  </label>
                </div>

                {/* Submit button */}
                <div className="pt-4">
                  <button
                    type="submit"
                    className="w-full bg-blue-600 hover:bg-blue-500 text-white font-extrabold text-xs py-3.5 rounded-lg transition-colors cursor-pointer flex items-center justify-center uppercase tracking-widest"
                    id="contact-form-submit-btn"
                  >
                    <Send className="w-4 h-4 mr-2" />
                    Submit Intake Form
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>
      </section>

    </div>
  );
}
