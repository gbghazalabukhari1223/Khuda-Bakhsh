/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect } from 'react';
import { PageId } from './types';
import Header from './components/Header';
import Footer from './components/Footer';
import MobileContactBar from './components/MobileContactBar';

// Import Pages
import Home from './pages/Home';
import About from './pages/About';
import Services from './pages/Services';
import ServiceDetail from './pages/ServiceDetail';
import Gallery from './pages/Gallery';
import BeforeAfter from './pages/BeforeAfter';
import AreasCovered from './pages/AreasCovered';
import FAQsPage from './pages/FAQsPage';
import Contact from './pages/Contact';
import Policies from './pages/Policies';

export default function App() {
  // Navigation Routing State (Home by default)
  const [activePage, setActivePage] = useState<PageId>('home');
  
  // Theme State (Defaulting to dark as requested by design directives)
  const [theme, setTheme] = useState<'dark' | 'light'>(() => {
    const saved = localStorage.getItem('swr-theme');
    if (saved === 'light' || saved === 'dark') {
      return saved;
    }
    return 'dark'; // Dark Charcoal default
  });

  // Synchronize CSS class on Document element
  useEffect(() => {
    const root = window.document.documentElement;
    if (theme === 'dark') {
      root.classList.add('dark');
      root.style.backgroundColor = '#0a0a0a'; // match deep black
    } else {
      root.classList.remove('dark');
      root.style.backgroundColor = '#fcfcfc'; // match soft off-white
    }
    localStorage.setItem('swr-theme', theme);
  }, [theme]);

  const handleToggleTheme = () => {
    setTheme(prev => prev === 'dark' ? 'light' : 'dark');
  };

  const handleNavigate = (pageId: PageId) => {
    setActivePage(pageId);
  };

  // Render Page Content based on current Active ID
  const renderPageContent = () => {
    switch (activePage) {
      case 'home':
        return <Home onNavigate={handleNavigate} />;
      case 'about-us':
        return <About onNavigate={handleNavigate} />;
      case 'services':
        return <Services onNavigate={handleNavigate} />;
      case 'gallery':
        return <Gallery onNavigate={handleNavigate} />;
      case 'before-after':
        return <BeforeAfter onNavigate={handleNavigate} />;
      case 'areas-covered':
        return <AreasCovered onNavigate={handleNavigate} />;
      case 'faqs':
        return <FAQsPage onNavigate={handleNavigate} />;
      case 'contact':
        return <Contact onNavigate={handleNavigate} />;
      
      // Policy triggers
      case 'privacy-policy':
      case 'terms-and-conditions':
      case 'cookie-policy':
      case 'sitemap':
        return <Policies activePolicy={activePage} onNavigate={handleNavigate} />;
      
      // Handle 12 Dynamic Service Pages
      default:
        return <ServiceDetail serviceId={activePage} onNavigate={handleNavigate} />;
    }
  };

  return (
    <div className={`min-h-screen flex flex-col font-sans transition-colors duration-300 ${
      theme === 'dark' ? 'bg-neutral-950 text-white' : 'bg-neutral-50 text-neutral-900'
    }`}>
      {/* Header element */}
      <Header 
        activePage={activePage} 
        onNavigate={handleNavigate} 
        theme={theme} 
        onToggleTheme={handleToggleTheme} 
      />

      {/* Main page content area */}
      <main className="flex-1">
        {renderPageContent()}
      </main>

      {/* Footer element */}
      <Footer onNavigate={handleNavigate} />

      {/* Mobile Sticky Contact Bar */}
      <MobileContactBar />
    </div>
  );
}

