/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

export type PageId =
  | 'home'
  | 'about-us'
  | 'services'
  | 'house-clearance-plymouth'
  | 'rubbish-removal-plymouth'
  | 'waste-removal-plymouth'
  | 'garden-waste-removal-plymouth'
  | 'commercial-waste-removal-plymouth'
  | 'office-clearance-plymouth'
  | 'garage-clearance-plymouth'
  | 'furniture-removal-plymouth'
  | 'builders-waste-removal-plymouth'
  | 'man-and-van-plymouth'
  | 'recycling-services-plymouth'
  | 'same-day-waste-collection-plymouth'
  | 'gallery'
  | 'before-after'
  | 'areas-covered'
  | 'faqs'
  | 'contact'
  | 'privacy-policy'
  | 'terms-and-conditions'
  | 'cookie-policy'
  | 'sitemap';

export interface Service {
  id: PageId;
  number: string;
  category: string;
  title: string;
  shortDesc: string;
  description: string;
  imageUrl: string;
  items: string[];
  features: string[];
  whoIsItFor: string[];
  accessConsiderations: string[];
  faqs: { question: string; answer: string }[];
}

export interface GalleryItem {
  id: string;
  title: string;
  category: 'vehicles' | 'household' | 'garden' | 'furniture' | 'builders' | 'commercial' | 'spaces';
  imageUrl: string;
  description: string;
}

export interface BeforeAfterStory {
  id: string;
  category: string;
  title: string;
  description: string;
  location: string;
  beforeImageUrl: string;
  afterImageUrl: string;
  details: string[];
}

export interface FAQ {
  id: string;
  question: string;
  answer: string;
}

export interface ContactFormData {
  name: string;
  phone: string;
  email: string;
  postcode: string;
  service: string;
  description: string;
  preferredDate: string;
  consent: boolean;
}
