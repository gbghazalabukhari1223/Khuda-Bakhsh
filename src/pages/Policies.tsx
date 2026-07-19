/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React from 'react';
import { PageId } from '../types';
import { BUSINESS_INFO, SERVICES } from '../data';
import { Shield, FileText, Globe, ListCollapse, ChevronRight } from 'lucide-react';

interface PoliciesProps {
  activePolicy: 'privacy-policy' | 'terms-and-conditions' | 'cookie-policy' | 'sitemap';
  onNavigate: (pageId: PageId) => void;
}

export default function Policies({ activePolicy, onNavigate }: PoliciesProps) {
  const handleLinkClick = (pageId: PageId) => {
    onNavigate(pageId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* 1. Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-12 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {activePolicy === 'privacy-policy' && (
            <>
              <Shield className="w-10 h-10 text-blue-500 mx-auto mb-3" />
              <h1 className="text-2xl sm:text-4xl font-extrabold text-white uppercase tracking-tight">Privacy Policy</h1>
              <p className="mt-2 text-xs text-neutral-400">UK GDPR Compliance and Data Controller Information</p>
            </>
          )}
          {activePolicy === 'terms-and-conditions' && (
            <>
              <FileText className="w-10 h-10 text-blue-500 mx-auto mb-3" />
              <h1 className="text-2xl sm:text-4xl font-extrabold text-white uppercase tracking-tight">Terms & Conditions</h1>
              <p className="mt-2 text-xs text-neutral-400">Clearance Volume and Payment Policies</p>
            </>
          )}
          {activePolicy === 'cookie-policy' && (
            <>
              <Globe className="w-10 h-10 text-blue-500 mx-auto mb-3" />
              <h1 className="text-2xl sm:text-4xl font-extrabold text-white uppercase tracking-tight">Cookie Policy</h1>
              <p className="mt-2 text-xs text-neutral-400">State and Theme Preserving Declarations</p>
            </>
          )}
          {activePolicy === 'sitemap' && (
            <>
              <ListCollapse className="w-10 h-10 text-blue-500 mx-auto mb-3" />
              <h1 className="text-2xl sm:text-4xl font-extrabold text-white uppercase tracking-tight">Visual Website Sitemap</h1>
              <p className="mt-2 text-xs text-neutral-400">Complete Directory of all active URLs</p>
            </>
          )}
        </div>
      </section>

      {/* 2. Policy Body */}
      <section className="py-16 max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-neutral-400 text-xs sm:text-sm leading-relaxed space-y-8">
        
        {activePolicy === 'privacy-policy' && (
          <div className="space-y-6">
            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">1. Who is the Data Controller?</h2>
              <p>
                Supreme Waste Removal Services Ltd, operating from <strong>53 Duncombe Avenue, Plymouth, PL5 2JT, United Kingdom</strong>, acts as the primary data controller for all personal information submitted through this website under the UK General Data Protection Regulation (UK GDPR).
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">2. What Information Do We Collect?</h2>
              <p>
                We only collect minimal data required to estimate clearance volumes and schedule vehicle routes. This includes: (a) Your Name, (b) Contact Phone Number, (c) Email Address, and (d) Waste Collection Postcode. If you send pictures of your clutter over WhatsApp, we process those files solely to evaluate the job size.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">3. Storage & Third-Party Sharing</h2>
              <p>
                We value your security. We never share your contact numbers or email logs with advertising networks or third-party marketing companies. Data is processed locally to handle your booking, write transfer invoices, and coordinate with authorized waste sorting stations.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">4. Your GDPR Rights</h2>
              <p>
                You have the absolute right to request access to, correction of, or permanent deletion of your contact logs at any time. To execute these rights, please email us directly at <strong>supremewaste011@gmail.com</strong>.
              </p>
            </div>
          </div>
        )}

        {activePolicy === 'terms-and-conditions' && (
          <div className="space-y-6">
            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">1. Estimation Adjustments</h2>
              <p>
                All photograph-based estimates represent a non-binding price projection based on the visible clutter shown. If our active loading crew determines the actual volume or weight exceeds what was shown in the pictures, we reserve the right to adjust the rate on site. You will always be informed before loading begins.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">2. Restricted Materials</h2>
              <p>
                Under Environmental Agency regulations, we are strictly prohibited from loading hazardous materials including raw asbestos insulation, toxic chemicals, liquid fuels, or gas cylinders. Please declare any suspicious paint cans or canisters to our desk beforehand.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">3. Payment Terms</h2>
              <p>
                Payment must be settled immediately upon successful completion of the clearance job. We accept credit/debit cards, bank transfers, or cash payment. Electronic VAT invoices are generated promptly and delivered via email or text.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">4. Access Requirements</h2>
              <p>
                It is the client's responsibility to arrange parking permits or driveway access for our high-sided transit vehicles. If narrow paths or stairwells require excessive walking time, minor loading fees may apply.
              </p>
            </div>
          </div>
        )}

        {activePolicy === 'cookie-policy' && (
          <div className="space-y-6">
            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">1. Simple & Non-Invasive Cookies</h2>
              <p>
                To provide a fast and modern browser interface, we use standard client-side storage technologies (such as cookies and local storage keys). These keys are purely functional and do not track your browsing histories across external websites.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">2. Preserving Visual Themes</h2>
              <p>
                Our system uses local storage to store your visual appearance preference (Dark vs Light theme) so that the website remains styled correctly when you refresh the page or click between service tabs.
              </p>
            </div>

            <div>
              <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-2">3. Managing Preferences</h2>
              <p>
                You can block or purge browser cookies inside your individual browser settings at any time, though this may cause the website theme toggles to revert to system defaults.
              </p>
            </div>
          </div>
        )}

        {activePolicy === 'sitemap' && (
          <div className="bg-neutral-900 border border-neutral-850 p-6 sm:p-8 rounded-xl">
            <h2 className="text-base font-extrabold text-white uppercase tracking-wider mb-4 border-b border-neutral-800 pb-2">
              Website URL Directory
            </h2>
            
            <div className="space-y-6">
              <div>
                <h3 className="text-xs font-bold text-blue-400 uppercase tracking-widest mb-2">Primary Pages</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs">
                  <button onClick={() => handleLinkClick('home')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Home Page</button>
                  <button onClick={() => handleLinkClick('about-us')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> About Us</button>
                  <button onClick={() => handleLinkClick('services')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> All Services Directory</button>
                  <button onClick={() => handleLinkClick('gallery')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Gallery Portfolio</button>
                  <button onClick={() => handleLinkClick('before-after')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Before & After Projects</button>
                  <button onClick={() => handleLinkClick('areas-covered')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Areas Covered</button>
                  <button onClick={() => handleLinkClick('faqs')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> FAQs Desk</button>
                  <button onClick={() => handleLinkClick('contact')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Contact Plymouth Office</button>
                </div>
              </div>

              <div>
                <h3 className="text-xs font-bold text-blue-400 uppercase tracking-widest mb-2">Individual Service Pages</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs">
                  {SERVICES.map((srv) => (
                    <button 
                      key={srv.id} 
                      onClick={() => handleLinkClick(srv.id)} 
                      className="text-left hover:text-white flex items-center text-neutral-400 hover:text-white"
                    >
                      <ChevronRight className="w-3.5 h-3.5 text-neutral-600 mr-1" />
                      {srv.title}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <h3 className="text-xs font-bold text-blue-400 uppercase tracking-widest mb-2">Legal Declarations</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs">
                  <button onClick={() => handleLinkClick('privacy-policy')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Privacy Policy</button>
                  <button onClick={() => handleLinkClick('terms-and-conditions')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Terms & Conditions</button>
                  <button onClick={() => handleLinkClick('cookie-policy')} className="text-left hover:text-white flex items-center"><ChevronRight className="w-3.5 h-3.5 text-blue-500 mr-1" /> Cookie Policy</button>
                </div>
              </div>
            </div>
          </div>
        )}
      </section>

    </div>
  );
}
