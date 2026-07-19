/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect, useRef } from 'react';
import { PageId } from '../types';
import { BUSINESS_INFO, SERVICES } from '../data';
import logoImg from '../assets/logo.jpg';
import { 
  Phone, 
  Facebook, 
  Menu, 
  X, 
  ChevronDown, 
  Sun, 
  Moon, 
  FileText, 
  Trash2, 
  Home, 
  Truck, 
  Calendar, 
  Sparkles, 
  MapPin,
  MessageSquare
} from 'lucide-react';

interface HeaderProps {
  activePage: PageId;
  onNavigate: (pageId: PageId) => void;
  theme: 'dark' | 'light';
  onToggleTheme: () => void;
}

export default function Header({ activePage, onNavigate, theme, onToggleTheme }: HeaderProps) {
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isMegaMenuOpen, setIsMegaMenuOpen] = useState(false);
  const megaMenuRef = useRef<HTMLDivElement>(null);
  const mobileMenuRef = useRef<HTMLDivElement>(null);

  // Monitor scroll height to trigger glass shrink effect
  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 40) {
        setIsScrolled(true);
      } else {
        setIsScrolled(false);
      }
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Handle key listeners for escape key
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setIsMegaMenuOpen(false);
        setIsMobileMenuOpen(false);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Prevent background scrolling when mobile menu is open
  useEffect(() => {
    if (isMobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [isMobileMenuOpen]);

  // Close mega menu on clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (megaMenuRef.current && !megaMenuRef.current.contains(event.target as Node)) {
        setIsMegaMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleLinkClick = (pageId: PageId) => {
    onNavigate(pageId);
    setIsMegaMenuOpen(false);
    setIsMobileMenuOpen(false);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Helper to determine active state of navigation links
  const isLinkActive = (pageId: PageId) => {
    if (pageId === 'home' && activePage === 'home') return true;
    if (pageId === 'services' && (activePage === 'services' || SERVICES.some(s => s.id === activePage))) return true;
    return activePage === pageId;
  };

  const getServiceIcon = (index: number) => {
    switch (index % 5) {
      case 0: return <Home className="w-4 h-4 text-blue-500 mr-2 shrink-0" />;
      case 1: return <Trash2 className="w-4 h-4 text-blue-500 mr-2 shrink-0" />;
      case 2: return <Truck className="w-4 h-4 text-blue-500 mr-2 shrink-0" />;
      case 3: return <Calendar className="w-4 h-4 text-blue-500 mr-2 shrink-0" />;
      default: return <Sparkles className="w-4 h-4 text-blue-500 mr-2 shrink-0" />;
    }
  };

  return (
    <header className="w-full z-50 transition-all duration-300 swr-global-header">
      {/* TOP CONTACT BAR */}
      <div className="w-full bg-neutral-950 text-neutral-400 border-b border-neutral-800 text-xs py-2 px-4 sm:px-6 lg:px-8 z-55">
        <div className="max-w-7xl mx-auto flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <span className="flex items-center">
              <span className="relative flex h-2 w-2 mr-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
              </span>
              <span className="font-medium text-neutral-300">Local waste removal across Plymouth</span>
            </span>
            <span className="hidden md:inline-block text-neutral-500">|</span>
            <span className="hidden md:inline-block">Same-day slots subject to availability</span>
          </div>
          
          <div className="flex items-center space-x-5">
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`} 
              className="flex items-center text-neutral-300 hover:text-blue-400 transition-colors duration-150 font-medium"
              id="top-bar-phone"
            >
              <Phone className="w-3.5 h-3.5 text-blue-500 mr-1.5" />
              <span>07940 598 976</span>
            </a>
            <a 
              href={BUSINESS_INFO.facebook} 
              target="_blank" 
              rel="noreferrer" 
              className="text-neutral-400 hover:text-blue-500 transition-colors duration-150 flex items-center"
              aria-label="Follow us on Facebook"
              id="top-bar-facebook"
            >
              <Facebook className="w-4 h-4" />
            </a>
          </div>
        </div>
      </div>

      {/* MAIN NAVIGATION HEADER */}
      <nav className={`w-full sticky top-0 transition-all duration-300 ${
        isScrolled 
          ? 'bg-neutral-900/90 dark:bg-neutral-950/90 backdrop-blur-md shadow-lg border-b border-neutral-800/80 py-3' 
          : 'bg-neutral-900 dark:bg-neutral-950 border-b border-neutral-800/40 py-5'
      }`}>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex items-center justify-between">
          {/* Logo */}
          <button 
            onClick={() => handleLinkClick('home')}
            className="flex items-center text-left focus:outline-none focus:ring-2 focus:ring-blue-500 rounded gap-3 group"
            aria-label="Supreme Waste Removal Home"
            id="logo-button"
          >
            <div className="relative w-10 h-10 sm:w-11 sm:h-11 bg-white rounded-lg p-0.5 shadow-md flex items-center justify-center overflow-hidden shrink-0 border border-neutral-800">
              <img 
                src={logoImg} 
                alt="Supreme Waste Removal Logo" 
                className="w-full h-full object-contain"
                referrerPolicy="no-referrer"
              />
            </div>
            <div className="flex flex-col">
              <span className="font-extrabold text-lg sm:text-xl lg:text-2xl tracking-tight text-white flex items-center group-hover:text-blue-400 transition-colors">
                SUPREME<span className="text-blue-500 font-light ml-1.5">WASTE</span>
              </span>
              <span className="text-[9px] sm:text-[10px] tracking-widest text-neutral-400 uppercase font-semibold">
                Removal Services Ltd
              </span>
            </div>
          </button>

          {/* Desktop Navigation Link Menu */}
          <div className="hidden lg:flex items-center space-x-1">
            <button
              onClick={() => handleLinkClick('home')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('home') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-home"
            >
              Home
            </button>
            <button
              onClick={() => handleLinkClick('about-us')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('about-us') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-about"
            >
              About
            </button>

            {/* SERVICES DROPDOWN TRIGGER */}
            <div className="relative" ref={megaMenuRef}>
              <button
                onClick={() => setIsMegaMenuOpen(!isMegaMenuOpen)}
                className={`px-3 py-2 text-sm font-medium rounded-md transition-colors flex items-center ${
                  isLinkActive('services') 
                    ? 'text-blue-400 bg-neutral-800/40' 
                    : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
                }`}
                aria-expanded={isMegaMenuOpen}
                id="nav-services-trigger"
              >
                Services
                <ChevronDown className={`w-4 h-4 ml-1 transition-transform duration-200 ${isMegaMenuOpen ? 'rotate-185' : 'rotate-0'}`} />
              </button>

              {/* SERVICES MEGA MENU (12 services) */}
              {isMegaMenuOpen && (
                <div className="absolute right-1/2 translate-x-1/2 mt-3 w-[680px] bg-neutral-900 border border-neutral-800 rounded-lg shadow-2xl p-6 grid grid-cols-2 gap-4 animate-in fade-in slide-in-from-top-3 duration-200 z-50">
                  <div className="col-span-2 border-b border-neutral-800 pb-3 mb-1 flex justify-between items-center">
                    <span className="text-xs font-semibold uppercase tracking-wider text-blue-400">Our Clearance & Removal Services</span>
                    <button 
                      onClick={() => handleLinkClick('services')}
                      className="text-xs text-neutral-400 hover:text-white underline"
                      id="view-all-services-link"
                    >
                      View All Directory
                    </button>
                  </div>
                  {SERVICES.map((srv, idx) => (
                    <button
                      key={srv.id}
                      onClick={() => handleLinkClick(srv.id)}
                      className="flex items-start text-left p-2 rounded-md hover:bg-neutral-800/50 transition-all group"
                      id={`mega-service-${srv.id}`}
                    >
                      {getServiceIcon(idx)}
                      <div>
                        <div className="text-sm font-semibold text-neutral-100 group-hover:text-blue-400 transition-colors">
                          {srv.title.replace(' Plymouth', '')}
                        </div>
                        <div className="text-xs text-neutral-400 line-clamp-1">
                          {srv.shortDesc}
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>

            <button
              onClick={() => handleLinkClick('gallery')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('gallery') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-gallery"
            >
              Gallery
            </button>
            <button
              onClick={() => handleLinkClick('before-after')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('before-after') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-before-after"
            >
              Before & After
            </button>
            <button
              onClick={() => handleLinkClick('areas-covered')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('areas-covered') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-areas"
            >
              Areas Covered
            </button>
            <button
              onClick={() => handleLinkClick('faqs')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('faqs') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-faqs"
            >
              FAQs
            </button>
            <button
              onClick={() => handleLinkClick('contact')}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isLinkActive('contact') 
                  ? 'text-blue-400 bg-neutral-800/40' 
                  : 'text-neutral-300 hover:text-white hover:bg-neutral-800/20'
              }`}
              id="nav-contact"
            >
              Contact
            </button>
          </div>

          {/* Quote Button & Theme Switcher */}
          <div className="hidden lg:flex items-center space-x-4">
            <button
              onClick={onToggleTheme}
              className="p-2 text-neutral-400 hover:text-blue-400 rounded-lg hover:bg-neutral-800/50 transition-colors focus:outline-none"
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
              id="theme-switcher-desktop"
            >
              {theme === 'dark' ? <Sun className="w-5 h-5 text-amber-400" /> : <Moon className="w-5 h-5 text-neutral-400" />}
            </button>

            <a
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-blue-600 hover:bg-blue-500 text-white font-semibold text-sm py-2.5 px-5 rounded-lg transition-all duration-150 flex items-center shadow-md shadow-blue-900/10"
              id="get-quote-desktop"
            >
              <MessageSquare className="w-4 h-4 mr-2" />
              Get a Quote
            </a>
          </div>

          {/* Mobile Right Icons (Hamburger, Theme Toggle) */}
          <div className="flex items-center lg:hidden space-x-2">
            <button
              onClick={onToggleTheme}
              className="p-2 text-neutral-400 hover:text-blue-400 rounded-lg hover:bg-neutral-800/50 transition-colors focus:outline-none"
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
              id="theme-switcher-mobile"
            >
              {theme === 'dark' ? <Sun className="w-5 h-5 text-amber-400" /> : <Moon className="w-5 h-5 text-neutral-400" />}
            </button>

            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="p-2 text-neutral-300 hover:text-white hover:bg-neutral-850 rounded-lg transition-colors focus:outline-none"
              aria-label={isMobileMenuOpen ? "Close menu" : "Open menu"}
              id="mobile-menu-hamburger"
            >
              {isMobileMenuOpen ? <X className="w-6 h-6 text-blue-500" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </div>

        {/* MOBILE SLIDE-DOWN DRAWER MENU */}
        {isMobileMenuOpen && (
          <div className="lg:hidden absolute top-full left-0 w-full bg-neutral-900 border-b border-neutral-800 p-6 z-40 max-h-[85vh] overflow-y-auto animate-in fade-in slide-in-from-top-4 duration-250">
            <div className="flex flex-col space-y-3">
              <button
                onClick={() => handleLinkClick('home')}
                className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                  activePage === 'home' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                }`}
                id="mobile-nav-home"
              >
                Home
              </button>
              <button
                onClick={() => handleLinkClick('about-us')}
                className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                  activePage === 'about-us' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                }`}
                id="mobile-nav-about"
              >
                About Us
              </button>
              
              {/* COLLAPSIBLE SERVICES ACCORDION FOR MOBILE */}
              <div className="border-t border-neutral-800 pt-2 mt-1">
                <div className="px-3 py-2 text-xs font-semibold tracking-wider text-neutral-500 uppercase">
                  Clearance Services
                </div>
                <div className="grid grid-cols-1 gap-1 pl-3">
                  {SERVICES.map((srv) => (
                    <button
                      key={srv.id}
                      onClick={() => handleLinkClick(srv.id)}
                      className={`w-full text-left py-1.5 px-3 rounded-md text-sm font-medium ${
                        activePage === srv.id ? 'text-blue-400 bg-neutral-850' : 'text-neutral-400 hover:text-white'
                      }`}
                      id={`mobile-nav-service-${srv.id}`}
                    >
                      {srv.title.replace(' Plymouth', '')}
                    </button>
                  ))}
                  <button
                    onClick={() => handleLinkClick('services')}
                    className="w-full text-left py-2 px-3 rounded-md text-sm font-bold text-blue-500 hover:text-blue-400"
                    id="mobile-nav-all-services"
                  >
                    View All Services Directory →
                  </button>
                </div>
              </div>

              <div className="border-t border-neutral-800 pt-2 mt-1">
                <button
                  onClick={() => handleLinkClick('gallery')}
                  className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                    activePage === 'gallery' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                  }`}
                  id="mobile-nav-gallery"
                >
                  Gallery Portfolio
                </button>
                <button
                  onClick={() => handleLinkClick('before-after')}
                  className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                    activePage === 'before-after' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                  }`}
                  id="mobile-nav-before-after"
                >
                  Before & After Projects
                </button>
                <button
                  onClick={() => handleLinkClick('areas-covered')}
                  className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                    activePage === 'areas-covered' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                  }`}
                  id="mobile-nav-areas"
                >
                  Areas Covered
                </button>
                <button
                  onClick={() => handleLinkClick('faqs')}
                  className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                    activePage === 'faqs' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                  }`}
                  id="mobile-nav-faqs"
                >
                  FAQs Accordion
                </button>
                <button
                  onClick={() => handleLinkClick('contact')}
                  className={`w-full text-left py-2 px-3 rounded-md text-base font-semibold ${
                    activePage === 'contact' ? 'text-blue-400 bg-neutral-800' : 'text-neutral-200 hover:text-white'
                  }`}
                  id="mobile-nav-contact"
                >
                  Contact Plymouth Office
                </button>
              </div>

              <div className="border-t border-neutral-800 pt-4 flex flex-col space-y-3">
                <a
                  href={BUSINESS_INFO.whatsappLink}
                  target="_blank"
                  rel="noreferrer"
                  className="bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-center py-3 px-4 rounded-lg text-sm flex items-center justify-center transition-colors"
                  id="mobile-menu-whatsapp"
                >
                  <MessageSquare className="w-4 h-4 mr-2" />
                  Chat on WhatsApp
                </a>
                <a
                  href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
                  className="bg-blue-600 hover:bg-blue-500 text-white font-bold text-center py-3 px-4 rounded-lg text-sm flex items-center justify-center transition-colors"
                  id="mobile-menu-phone"
                >
                  <Phone className="w-4 h-4 mr-2" />
                  Call 07940 598 976
                </a>
              </div>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
}
