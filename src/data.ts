/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { Service, GalleryItem, BeforeAfterStory, FAQ } from './types';
import vanImg from './assets/van.jpg';

export const BUSINESS_INFO = {
  name: "Supreme Waste Removal Services Ltd",
  phone: "07940 598 976",
  phoneFormatted: "+44 7940 598 976",
  email: "supremewaste011@gmail.com",
  address: "53 Duncombe Avenue, Plymouth, PL5 2JT, United Kingdom",
  facebook: "https://www.facebook.com/share/1D5v8ahPXi/?mibextid=wwXIfr",
  website: "https://supremewasteremovalltd.uk/",
  whatsappLink: "https://wa.me/447940598976?text=Hello%20Supreme%20Waste%20Removal%2C%20I%20would%20like%20a%20quote.%0A%0AName%3A%20%0APostcode%3A%20%0AService%20required%3A%20%0AItems%20or%20waste%3A%20%0AAccess%20details%3A%20%0APreferred%20date%3A%20",
  location: "Plymouth, Devon & East Cornwall",
};

export const AREAS_COVERED = [
  { name: "Plymouth", postcode: "PL1, PL2, PL3, PL4, PL5, PL6, PL9", availability: "Immediate Slots Available" },
  { name: "Plympton", postcode: "PL7", availability: "Same-Day / Next-Day" },
  { name: "Plymstock", postcode: "PL9", availability: "Same-Day / Next-Day" },
  { name: "Saltash", postcode: "PL12", availability: "Route Dependent" },
  { name: "Torpoint", postcode: "PL11", availability: "Route Dependent" },
  { name: "Ivybridge", postcode: "PL21", availability: "Daily Routes" },
  { name: "Tavistock", postcode: "PL19", availability: "Route Dependent" },
];

export const GENERAL_FAQS: FAQ[] = [
  {
    id: "faq-1",
    question: "How do I request a quotation?",
    answer: "The easiest and fastest way to get a quote is via WhatsApp. Simply snap a few clear photographs of your waste, note your collection postcode, and send them to 07940 598 976. Our team will review the photos and send back an accurate price estimate shortly."
  },
  {
    id: "faq-2",
    question: "Can you collect a single bulky item?",
    answer: "Absolutely. We collect single bulky items such as double mattresses, large sofas, washing machines, or fridge-freezers. This is a cost-effective alternative to skip hire when you only have one or two large pieces of furniture to clear."
  },
  {
    id: "faq-3",
    question: "Is same-day waste collection available?",
    answer: "Yes, we offer same-day response times across Plymouth, subject to vehicle routes and availability. It is best to contact us early in the morning via phone or WhatsApp with photographs so we can book you into our active schedules."
  },
  {
    id: "faq-4",
    question: "Do I need to hire a skip instead?",
    answer: "With Supreme Waste Removal, you do not need to deal with skip permits, road space issues, or loading the heavy debris yourself. We load everything onto our modern vehicles, clean up after the job is done, and charge only for the volume of waste we actually remove. It is faster, cheaper, and cleaner."
  },
  {
    id: "faq-5",
    question: "What types of waste can be collected?",
    answer: "We collect most general domestic and commercial waste including household junk, furniture, appliances, garden trimmings, soil, brick debris, timber, cardboard, packaging, and carpets. However, we cannot collect hazardous substances like raw asbestos, industrial chemicals, or liquid fuel."
  },
  {
    id: "faq-6",
    question: "Which areas do you cover?",
    answer: "Our primary hub is Plymouth, United Kingdom. We also cover surrounding suburbs and nearby towns including Plympton, Plymstock, Saltash, Torpoint, Ivybridge, Tavistock, and adjacent areas in Devon and East Cornwall depending on the schedule."
  },
  {
    id: "faq-7",
    question: "What information should I send with my photographs?",
    answer: "For a fast, firm quote, send: (1) clear photographs showing the entire pile of waste, (2) your full collection postcode, (3) a brief note if any items are exceptionally heavy, and (4) parking or access notes, such as narrow driveways or flat location on the third floor."
  },
  {
    id: "faq-8",
    question: "Can you clear waste from flats or properties with stairs?",
    answer: "Yes, we handle clearances from flats, apartments, and terraced houses with complex layouts. We just ask that you inform us about the stairs, lift availability, or long walking distances in advance so we can allocate the appropriate staff and timing."
  }
];

export const SERVICES: Service[] = [
  {
    id: 'house-clearance-plymouth',
    number: '01',
    category: 'Domestic Clearance',
    title: 'House Clearance Plymouth',
    shortDesc: 'Complete residential property clearances for homeowners, landlords, probate, and tenancies.',
    description: 'We provide full and partial house clearances across Plymouth and surrounding areas. Whether you are dealing with a deceased estate, sorting through years of accumulated belongings, clearing out a rental property at the end of a tenancy, or preparing a house for resale, our team handles everything with care and sensitivity. We do all the heavy lifting, loading, and disposal, leaving the property swept clean and ready for its next chapter.',
    imageUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=800&q=80',
    items: [
      'Heavy furniture (sofas, wardrobes, beds, tables)',
      'Carpets, rugs, underlays, and old curtains',
      'White goods, cookers, washing machines, fridge-freezers',
      'Bric-a-brac, stored boxes, clothing, and kitchenware'
    ],
    features: [
      'Empathetic, fully insured professional team',
      'Swept clean guarantee on completed clearances',
      'Environmentally conscious sorting for reusable items',
      'Liaison with estate agents, solicitors, or landlords if required'
    ],
    whoIsItFor: [
      'Homeowners preparing a property for sale or renovation',
      'Landlords dealing with abandoned items or end-of-tenancy cleans',
      'Families handling sensitive probate or bereavement property clearance',
      'Tenants moving house or downsizing to a smaller property'
    ],
    accessConsiderations: [
      'Please let us know if the property is a flat with stairs or has no lift.',
      'Inform us about any local parking permits or narrow streets that might restrict van access.',
      'We can collect keys from estate agents or solicitors if you are unable to attend the clearance.'
    ],
    faqs: [
      { question: "Do I need to pack everything into boxes first?", answer: "No, you do not need to pack everything. Our team can clear items directly from cupboards, drawers, lofts, or basements, saving you hours of stressful preparation work." },
      { question: "How long does a typical house clearance take?", answer: "A standard 2 or 3-bedroom house clearance is usually completed in one full day, depending on the volume of accumulated materials." }
    ]
  },
  {
    id: 'rubbish-removal-plymouth',
    number: '02',
    category: 'Rapid Waste Collection',
    title: 'Rubbish Removal Plymouth',
    shortDesc: 'Quick and efficient collection of general household waste, junk piles, and clutter.',
    description: 'If you have a pile of general rubbish building up in your yard, hallway, or driveway, our rubbish removal service is the perfect fast response. We collect all standard household rubbish, bin overflows, bagged waste, and unwanted junk that the council refuses to take. Instead of waiting for fortnightly bin days or loading dirty items into your own car, our responsive team handles the job quickly and leaves your space completely tidy.',
    imageUrl: 'https://images.unsplash.com/photo-1611284446314-60a58ac0deb9?auto=format&fit=crop&w=800&q=80',
    items: [
      'Black bag waste and general domestic rubbish',
      'Cardboard packaging, bubble wraps, and paper piles',
      'Old toys, worn-out carpets, and plastic storage tubs',
      'Clutter cleared from lofts, cellars, or cupboards'
    ],
    features: [
      'Faster and cleaner than skip hire',
      'Flexible pricing based on the actual space used',
      'Same-day response available for urgent collections',
      'All waste loaded and swept up by our team'
    ],
    whoIsItFor: [
      'Residents with excess rubbish after Christmas, parties, or major clear-outs',
      'Busy households with missed council waste collections',
      'People without a car or trailer to visit the local recycling depot',
      'Tenants clearing out rubbish before handing back keys'
    ],
    accessConsiderations: [
      'Ensure bags are accessible for easy carrying from the driveway or garden.',
      'Please keep any pets inside while our loaders are carrying heavy bags.',
      'Let us know if there are any sharp objects like glass or needles in the rubbish.'
    ],
    faqs: [
      { question: "Do you charge by the bag or the van load?", answer: "We quote based on the overall volume of rubbish. Send us a photo, and we will offer a highly competitive price for the entire pile." },
      { question: "Can you collect rubbish if I am not at home?", answer: "Yes, as long as the rubbish is placed outside in an accessible driveway or front garden, we can collect it and send you completion photos with payment handled online." }
    ]
  },
  {
    id: 'waste-removal-plymouth',
    number: '03',
    category: 'General Waste',
    title: 'Waste Removal Plymouth',
    shortDesc: 'Professional waste collection services for both domestic and light commercial clients.',
    description: 'Supreme Waste Removal Services Ltd is your dependable local partner for general waste disposal. We process, load, and transfer waste from residential and light commercial settings across Plymouth. Operating with strict adherence to environmental regulations and safe lifting practices, we ensure your waste is handled properly and directed to licensed disposal facilities where materials are sorted for maximum recycling potential.',
    imageUrl: 'https://images.unsplash.com/photo-1532996122724-e3c354a0b15b?auto=format&fit=crop&w=800&q=80',
    items: [
      'Mixed household waste and broken appliances',
      'Shed contents, old shelves, and tools',
      'Light timber scraps and plastic cladding',
      'Mattresses, bed frames, and bulky textiles'
    ],
    features: [
      'Fully licensed waste transfer operations',
      'Detailed electronic invoices and receipts',
      'Clean modern vehicles designed for waste handling',
      'Plymouth-based local business supporting local residents'
    ],
    whoIsItFor: [
      'Homeowners undergoing kitchen or bathroom replacements',
      'Property managers requiring general building maintenance clearances',
      'Local residents who want their waste handled responsibly',
      'Small business premises needing occasional waste clear-outs'
    ],
    accessConsiderations: [
      'Keep pathways clear of obstructions for safe carrying.',
      'Note if there are overhead cables or trees that might affect high-sided vans.',
      'Advise us of any shared access driveways with neighbours.'
    ],
    faqs: [
      { question: "Where does my waste go?", answer: "We take all waste to fully licensed commercial waste transfer stations in the Plymouth area, where it is sorted to extract recyclable elements." },
      { question: "Can you clear wet or heavy sodden waste?", answer: "Yes, we can, though we appreciate knowing about wet carpets or timber in advance as this adds significant weight." }
    ]
  },
  {
    id: 'garden-waste-removal-plymouth',
    number: '04',
    category: 'Outdoor Clearances',
    title: 'Garden Waste Removal Plymouth',
    shortDesc: 'Quick collection of green waste, branches, soil, broken fencing, and old garden furniture.',
    description: 'Reclaim your outdoor space with our specialized garden waste clearance. If you have been cutting back hedges, pruning trees, clearing overgrown flowerbeds, or tearing down an old rotting fence, we will quickly load and haul the debris away. We collect all forms of organic green waste, as well as general garden timber, broken paving slabs, old lawnmowers, and dilapidated metal sheds, allowing you to enjoy a beautiful, clean garden without multiple trips to the tip.',
    imageUrl: 'https://images.unsplash.com/photo-1585320806297-9794b3e4eeae?auto=format&fit=crop&w=800&q=80',
    items: [
      'Branches, logs, hedge trimmings, and grass clippings',
      'Soil, turf, organic garden soil, and compost',
      'Old sheds, decaying fences, and timber decking',
      'Broken garden furniture, plastic pots, and old trampolines'
    ],
    features: [
      '100% of organic green waste is sent to commercial composting facilities',
      'Shed dismantling services available upon request',
      'Avoids getting soil, mud, and sap in your clean car boot',
      'Equipped with heavy-duty tools, rakes, and brooms for cleanup'
    ],
    whoIsItFor: [
      'Keen gardeners who have undertaken large pruning or landscaping projects',
      'New homeowners inheritance of an overgrown, neglected garden',
      'Landlords clearing gardens before a property is leased out',
      'Busy families needing heavy outdoor items like old swings or trampolines removed'
    ],
    accessConsiderations: [
      'Please state if our team needs to carry waste through the house or if there is side access.',
      'Let us know if there are steep steps, slippery grassy banks, or muddy paths.',
      'Clearly define what needs to be removed and what plants should be preserved.'
    ],
    faqs: [
      { question: "Can you dismantle my old garden shed?", answer: "Yes, we offer a light dismantling service for wooden garden sheds and structures as part of our clearance." },
      { question: "Do I need to bag up leaves and small clippings?", answer: "It is highly helpful if leaves and loose grass clippings are bagged or piled together, but our team can shovel and sweep loose debris if needed." }
    ]
  },
  {
    id: 'commercial-waste-removal-plymouth',
    number: '05',
    category: 'Commercial Support',
    title: 'Commercial Waste Removal Plymouth',
    shortDesc: 'Flexible waste collection solutions for retail shops, commercial venues, and developers.',
    description: 'We offer dependable commercial waste collection designed to keep your business running smoothly without the high cost of long-term contract bins or bulky skips. From retail shops, hotels, and restaurants to local offices and light industrial units, we collect cardboard packaging, pallets, shopfitting debris, or promotional materials. Our scheduled or on-demand service operates efficiently, allowing you to focus on your clients while we maintain your site safety.',
    imageUrl: 'https://images.unsplash.com/photo-1530587191325-3db32d826c18?auto=format&fit=crop&w=800&q=80',
    items: [
      'Cardboard, shrink wrap, and pallet boards',
      'Shopfitting materials, plasterboard, and plastic fixtures',
      'Excess warehouse packaging and broken stock items',
      'Food service dry waste and restaurant materials'
    ],
    features: [
      'Flexible, on-demand collections mean no fixed monthly fees',
      'Full commercial waste transfer notes (WTN) provided',
      'Out-of-hours collections available to avoid customer disruption',
      'VAT invoices issued promptly upon job completion'
    ],
    whoIsItFor: [
      'High street shops with limited bin storage space',
      'Warehouses needing rapid clearance of packaging and wooden pallets',
      'Shopfitters executing modern commercial refurbishments',
      'Landlords preparing empty commercial premises for new tenants'
    ],
    accessConsiderations: [
      'Advise us of service bays, loading zones, or delivery restrictions.',
      'Specify any height limits in basement car parks or service yards.',
      'Let us know if we need to sign in with security or facilities management.'
    ],
    faqs: [
      { question: "Do you offer contract-free collections?", answer: "Yes! We operate on an on-demand, contract-free basis. You call us when you need waste removed, and you only pay for what we collect." },
      { question: "Are you fully insured for commercial clearances?", answer: "Yes, we carry extensive public liability insurance and are fully licensed to transport commercial waste materials." }
    ]
  },
  {
    id: 'office-clearance-plymouth',
    number: '06',
    category: 'Commercial Support',
    title: 'Office Clearance Plymouth',
    shortDesc: 'Efficient clearance of office furniture, paper, and general workplace materials.',
    description: 'When office layouts change, companies downsize, or furniture needs replacing, our office clearance team is ready to step in. We clear individual offices or complete multi-floor commercial buildings across Plymouth. We specialize in the safe dismantling and swift removal of metal desks, heavy cabinets, seating, partitioned dividers, files, and general workplace rubbish. Our goal is to minimize disruption to your ongoing business operations.',
    imageUrl: 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80',
    items: [
      'Desks, conference tables, and ergonomic office chairs',
      'Filing cabinets, shelving units, and metal lockers',
      'Archived paper documents, files, and cardboard boxes',
      'Workplace kitchen appliances, small fridges, and water dispensers'
    ],
    features: [
      'Eco-friendly route: metal furniture and paper are 100% recycled',
      'Safe manual lifting inside corporate and commercial spaces',
      'Flexible scheduling including evenings and weekends',
      'Thorough cleanup of office carpets and service areas'
    ],
    whoIsItFor: [
      'Businesses moving to modern new office buildings in Plymouth',
      'Companies updating old furniture with modern standing desks',
      'Liquidators or trustees clearing bankrupt commercial premises',
      'Facilities managers preparing offices for tenant handbacks'
    ],
    accessConsiderations: [
      'Note if service lifts are available and require booking in advance.',
      'Advise on timing constraints (e.g., quiet office hours, loading bay availability).',
      'Provide details on the best entrance to avoid disturbing other businesses.'
    ],
    faqs: [
      { question: "Can you recycle old metal desks and filing cabinets?", answer: "Yes, old metal furniture is fully recycled. We prioritize separating metal, plastics, and wood during our sorting process." },
      { question: "Can you work outside of normal business hours?", answer: "Yes, we can schedule office clearances for evenings or weekends to prevent any disruption to your staff and clients." }
    ]
  },
  {
    id: 'garage-clearance-plymouth',
    number: '07',
    category: 'Domestic Clearance',
    title: 'Garage Clearance Plymouth',
    shortDesc: 'Clearing old paint, tools, junk, and boxes to help you park or use your garage again.',
    description: 'Garages are prime magnets for unused clutter, slowly filling with unwanted boxes, broken tools, old paint tins, and discarded garden furniture until you can no longer fit your car or use the space. Our garage clearance service sweeps through the clutter, separating items, carrying away bulky junk, and cleaning up the space. Let our team do the heavy lifting so you can reclaim your garage for parking, a home workshop, or a clean storage area.',
    imageUrl: 'https://images.unsplash.com/photo-1595206133361-b1fe343e5e23?auto=format&fit=crop&w=800&q=80',
    items: [
      'Broken tools, workbenches, and old hardware',
      'Camping gear, bicycles, and outgrown garden toys',
      'Empty paint tins, plastic pots, and old car parts',
      'Rotting timber shelves, metal storage bins, and clutter'
    ],
    features: [
      'Complete clearance and sweep-up service',
      'Removal of old racking and heavy workbenches',
      'Responsible recycling of metal items and electronics',
      'Priced dynamically based on volume cleared'
    ],
    whoIsItFor: [
      'Homeowners preparing to install a home gym or workshop',
      'Sellers looking to present a tidy garage space to potential buyers',
      'Families clearing out garages ahead of property moves',
      'People needing help dismantling old shelving and racks'
    ],
    accessConsiderations: [
      'Please ensure the garage door is operational and can open fully.',
      'Let us know if there is vehicle access right up to the garage mouth.',
      'Highlight any sensitive items or chemicals that cannot be moved.'
    ],
    faqs: [
      { question: "Can you dispose of old tins of paint?", answer: "Yes, we can collect old paint tins, but they must be dry/hardened as we cannot carry liquid paint. Adding sawdust or paint hardener first is highly recommended." },
      { question: "Do you clear the cupboards inside the garage?", answer: "Yes, we clear all cupboards, drawers, workbenches, and overhead rafters as part of the service." }
    ]
  },
  {
    id: 'furniture-removal-plymouth',
    number: '08',
    category: 'Bulky Collection',
    title: 'Furniture Removal Plymouth',
    shortDesc: 'Single-item or full suite collections including sofas, wardrobes, beds, and dining sets.',
    description: 'Replacing old furniture can be an exciting update, but disposing of heavy, awkward items is often a physical nightmare. Our professional furniture removal team handles everything. We navigate narrow doorways, tight stairwells, and awkward corners to remove old sofas, armchairs, double beds, wardrobes, and heavy solid wood dining tables safely. We lift and carry everything, ensuring no damage to your walls or doors.',
    imageUrl: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=800&q=80',
    items: [
      'Three-piece suites, modular sofas, and armchairs',
      'Heavy wooden wardrobes, chests of drawers, and sideboards',
      'Beds, mattresses, divans, and metal bed frames',
      'Dining tables, chairs, display cabinets, and bookshelves'
    ],
    features: [
      'Expert two-man lifting team for heavy items',
      'Careful maneuvering to protect home interior surfaces',
      'Mattress and fabric recycling protocols',
      'Immediate WhatsApp photo estimates for individual pieces'
    ],
    whoIsItFor: [
      'Buyers of new furniture who need their old items removed quickly',
      'Downsizers clearing out pieces that won\'t fit their new home',
      'Landlords replacing worn-out furniture in student rentals',
      'Individuals unable to lift heavy items due to mobility concerns'
    ],
    accessConsiderations: [
      'Please check if large wardrobes need to be dismantled to fit down stairs.',
      'Inform us of tight hallway turns or narrow door frames.',
      'Make sure there is a clear path from the room to the front door.'
    ],
    faqs: [
      { question: "Do you collect dirty or stained mattresses?", answer: "Yes, we collect all mattresses regardless of their condition for proper recycling and fiber recovery." },
      { question: "Can you dismantle flat-pack wardrobes?", answer: "Yes, our team can dismantle large wooden wardrobes or bed frames if they are too big to carry out in one piece." }
    ]
  },
  {
    id: 'builders-waste-removal-plymouth',
    number: '09',
    category: 'Project Cleanups',
    title: 'Builders Waste Removal Plymouth',
    shortDesc: 'Collection of DIY debris, tiles, plasterboard, timber, and light trade renovation waste.',
    description: 'Home renovation projects, kitchen replacements, and bathroom updates generate an enormous amount of heavy, awkward building waste. Our builders waste collection service is specifically designed for DIYers and local tradespeople. We clear timber offcuts, ceramic tiles, old plasterboard, piping, broken brick fragments, and packaging materials. We load the heavy items directly from your driveway or property, saving you multiple trips to the commercial depot.',
    imageUrl: 'https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=800&q=80',
    items: [
      'Timber offcuts, floorboards, joists, and doors',
      'Ceramic tiles, old plasterboard, and insulation rolls',
      'Plastic trunking, metal pipes, and broken radiators',
      'Bags of rubble, concrete fragments, and heavy soil'
    ],
    features: [
      'Excellent alternative to expensive skip hire in narrow Plymouth streets',
      'No street permit or skip license required',
      'Pay only for the exact volume loaded',
      'Fast turnaround keeps your building site safe and tidy'
    ],
    whoIsItFor: [
      'Homeowners undertaking kitchen or bathroom refurbishments',
      'Tradespeople (plumbers, carpenters, tilers) wanting a clean workspace',
      'Landlords modernizing older properties between tenancies',
      'Developers preparing a site for decorating or handover'
    ],
    accessConsiderations: [
      'Please place heavy bricks and tiles as close to the parking area as possible.',
      'Inform us of plasterboard items, which must be kept separate from general waste.',
      'Advise us if our loaders must cross soft garden lawn with wheelbarrows.'
    ],
    faqs: [
      { question: "Do you have a weight limit for heavy rubble?", answer: "Yes, very heavy materials like solid brick, concrete, or wet soil are subject to weight-based limits. We will guide you on this when quoting." },
      { question: "Do you clear plasterboard?", answer: "Yes, we collect plasterboard, but regulations require it to be kept dry and separate from general mixed waste. Please let us know in advance." }
    ]
  },
  {
    id: 'man-and-van-plymouth',
    number: '10',
    category: 'Rapid Transport',
    title: 'Man and Van Services Plymouth',
    shortDesc: 'Affordable transport and loading support for small moves and furniture deliveries.',
    description: 'Need something moved from A to B but don\'t have the vehicle space? Our Man & Van services across Plymouth are perfect for small moves, furniture collection, student relocation, or single-item transport. We don\'t just drive the van—we help you load and unload, securing your belongings safely in our clean, transit-style vehicles. It is the hassle-free way to handle transport without expensive self-drive hire companies or complex logistics.',
    imageUrl: 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?auto=format&fit=crop&w=800&q=80',
    items: [
      'Small flat removals and student dorm relocation',
      'Collection of bulky store purchases (IKEA, B&Q, etc.)',
      'Transporting eBay or Facebook Marketplace sales',
      'Moving garden plants or heavy household planters'
    ],
    features: [
      'Experienced loader to assist with packing and carrying',
      'Clean, dry transit van with protective ties and blankets',
      'Direct, dedicated transport from pick-up to drop-off',
      'Highly competitive hourly or flat-rate options'
    ],
    whoIsItFor: [
      'Students moving into Plymouth University accommodation',
      'Buyers purchasing heavy furniture from local online marketplaces',
      'Flat dwellers moving down the street with a small collection of boxes',
      'Businesses needing rapid local courier transport for large components'
    ],
    accessConsiderations: [
      'Ensure there is clear, close parking at both the pickup and delivery addresses.',
      'Advise us if items are located in high-floor apartments without lifts.',
      'Dismantle larger furniture pieces prior to our arrival to speed up loading.'
    ],
    faqs: [
      { question: "Do you travel outside of Plymouth?", answer: "Yes, while we are based in Plymouth, we can transport items to nearby towns such as Saltash, Tavistock, Ivybridge, or further afield by arrangement." },
      { question: "Do you provide cardboard boxes?", answer: "We do not provide packing materials as standard, but we use high-quality transit blankets and heavy-duty straps in our van to secure everything." }
    ]
  },
  {
    id: 'recycling-services-plymouth',
    number: '11',
    category: 'Green Disposal',
    title: 'Recycling Services Plymouth',
    shortDesc: 'Responsible waste sorting to divert wood, cardboard, metal, and plastic from landfill.',
    description: 'At Supreme Waste Removal Services Ltd, we take our environmental responsibility seriously. Our dedicated recycling services focus on extracting maximum value from the collections we perform. Instead of taking mixed waste directly to landfill, we sort through the cargo to isolate cardboard, metal, plastics, timber, and electronic items. By partnering with specialist commercial recycling facilities in Plymouth, we ensure your unwanted waste is repurposed wherever practical.',
    imageUrl: 'https://images.unsplash.com/photo-1503596476-1c12a8ba09a9?auto=format&fit=crop&w=800&q=80',
    items: [
      'Cardboard packing, paper, and magazines',
      'Metal frames, appliances, piping, and radiators',
      'Clean wood, pallets, and timber decking',
      'Consumer electronics, old computers, and small appliances'
    ],
    features: [
      'Comprehensive sorting protocols on mixed collections',
      'Partnerships with authorized local recycling depots',
      'Electronic waste (WEEE) regulations compliance',
      'Diverting reusable furniture to local charity outlets'
    ],
    whoIsItFor: [
      'Environmentally conscious households and businesses',
      'Companies needing compliance records for their recycling targets',
      'Anyone looking to reduce their carbon footprint in Plymouth',
      'Local developers seeking green waste disposal audits'
    ],
    accessConsiderations: [
      'Please separate dry recyclables from wet waste where possible.',
      'Inform us of electrical items, which require distinct processing.',
      'Check if furniture is in good enough condition to be donated.'
    ],
    faqs: [
      { question: "How much of the waste is actually recycled?", answer: "We aim to recycle, reclaim, or divert from landfill up to 80% of the suitable materials we collect, working closely with modern sorting facilities." },
      { question: "Do you collect electronic waste like TVs?", answer: "Yes, we collect old TVs, monitors, computer towers, and white goods in accordance with standard WEEE recycling guidelines." }
    ]
  },
  {
    id: 'same-day-waste-collection-plymouth',
    number: '12',
    category: 'Rapid Waste Collection',
    title: 'Same-Day Waste Collection Plymouth',
    shortDesc: 'Urgent rubbish, waste, and clearance collection when time is of the essence.',
    description: 'When urgent situations arise—such as fly-tipped waste on your land, a sudden property sale completion, an unexpected move, or commercial bin failure—our same-day waste collection is the ultimate solution. Operating across Plymouth and nearby areas, we prioritize urgent requests. Simply contact us with your location, photographs, and description, and we will do our absolute best to dispatch a vehicle to clear your waste within hours.',
    imageUrl: 'https://images.unsplash.com/photo-1516574187841-cb9cc2ca948b?auto=format&fit=crop&w=800&q=80',
    items: [
      'Urgent household bin overflow or garden waste',
      'Fly-tipped rubbish piles requiring swift cleanups',
      'Last-minute flat or office move clearance debris',
      'Broken appliances blocking crucial access pathways'
    ],
    features: [
      'Priority scheduling for critical clearance situations',
      'Immediate phone or WhatsApp communication lines',
      'No premium surcharge—honest volume-based pricing',
      'Reliable, prompt, and highly responsive local team'
    ],
    whoIsItFor: [
      'Homeowners preparing for imminent property inspections or key handovers',
      'Businesses facing unexpected stock damage or bin overflows',
      'Landlords needing an immediate turnaround between tenants',
      'Victims of fly-tipping who need their premises secured quickly'
    ],
    accessConsiderations: [
      'Please remain contactable by phone to confirm our vehicle\'s arrival time.',
      'Ensure the waste is ready for loading and easily accessible to our team.',
      'Clear any parking spaces outside the property to facilitate rapid loading.'
    ],
    faqs: [
      { question: "What is the latest time I can book a same-day collection?", answer: "We recommend contacting us before 12:00 PM for the highest chance of securing a same-day slot. However, we always accommodate late-afternoon emergencies when possible." },
      { question: "Is there an extra fee for same-day callouts?", answer: "We do not charge an emergency surcharge. Our pricing remains based on the volume of waste we collect, making it fair and transparent." }
    ]
  }
];

export const GALLERY_ITEMS: GalleryItem[] = [
  {
    id: 'gal-1',
    title: 'Modern Transit Waste Removal Van',
    category: 'vehicles',
    imageUrl: vanImg,
    description: 'Our clean, high-sided waste collection van parked on a Plymouth residential driveway, ready for loading.'
  },
  {
    id: 'gal-2',
    title: 'Full House Clearance Sorting',
    category: 'household',
    imageUrl: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=800&q=80',
    description: 'Sorting and packing domestic items during a complete 3-bedroom property clearance in Plympton.'
  },
  {
    id: 'gal-3',
    title: 'Eco-Friendly Garden Waste Loading',
    category: 'garden',
    imageUrl: 'https://images.unsplash.com/photo-1585320806297-9794b3e4eeae?auto=format&fit=crop&w=800&q=80',
    description: 'Loading heaps of organic green hedge trimmings and tree branches for commercial composting.'
  },
  {
    id: 'gal-4',
    title: 'Bulky Sofa and Armchair Collection',
    category: 'furniture',
    imageUrl: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=800&q=80',
    description: 'Safely carrying out a large modular sofa from a tight terraced hallway in Plymouth without wall marks.'
  },
  {
    id: 'gal-5',
    title: 'Renovation Timber & Builders Waste',
    category: 'builders',
    imageUrl: 'https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=800&q=80',
    description: 'DIY builders waste collection, removing old kitchen cabinets, tiles, and plasterboard scrap.'
  },
  {
    id: 'gal-6',
    title: 'Commercial Office Layout Clearance',
    category: 'commercial',
    imageUrl: 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80',
    description: 'Clearing metal storage cabinets, partitioned panels, and unwanted desks from a Plymouth business park.'
  },
  {
    id: 'gal-7',
    title: 'Swept Clean Residential Garage',
    category: 'spaces',
    imageUrl: 'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?auto=format&fit=crop&w=800&q=80',
    description: 'The final view of a double garage space swept entirely clean after removing 15 years of accumulated clutter.'
  },
  {
    id: 'gal-8',
    title: 'Cardboard and Plastics Recycling sorting',
    category: 'spaces',
    imageUrl: 'https://images.unsplash.com/photo-1503596476-1c12a8ba09a9?auto=format&fit=crop&w=800&q=80',
    description: 'Cardboard boxes flattened and organized for transfer to the local recycling plant.'
  }
];

export const BEFORE_AFTER_STORIES: BeforeAfterStory[] = [
  {
    id: 'ba-1',
    category: 'Garden Clearance',
    title: 'Overgrown Ivy and Timber Clearance',
    description: 'A Plymouth residential back garden had accumulated piles of rotten timber decking, broken plastic pots, and extensive ivy clippings over several years.',
    location: 'Plymstock, Plymouth',
    beforeImageUrl: 'https://images.unsplash.com/photo-1584622650111-993a426fbf0a?auto=format&fit=crop&w=800&q=80',
    afterImageUrl: 'https://images.unsplash.com/photo-1585320806297-9794b3e4eeae?auto=format&fit=crop&w=800&q=80',
    details: [
      'Removed 3.5 cubic yards of mixed wood waste and organic materials',
      'Swept paths clean and raked the gravel border neat',
      'Completed in less than 2.5 hours by a 2-man team',
      'All green waste processed into local compost'
    ]
  },
  {
    id: 'ba-2',
    category: 'House Clearance',
    title: 'End of Tenancy Rental Clearance',
    description: 'A landlord was left with a flat full of abandoned bulky furniture, soiled carpets, and broken appliances following a rapid lease termination.',
    location: 'Stoke, Plymouth',
    beforeImageUrl: 'https://images.unsplash.com/photo-1611284446314-60a58ac0deb9?auto=format&fit=crop&w=800&q=80',
    afterImageUrl: 'https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?auto=format&fit=crop&w=800&q=80',
    details: [
      'Cleared bulky sofas, 2 double bed frames, and a broken cooker',
      'Uplifted damaged kitchen lino and bedroom underlay carpets',
      'Fully vacuumed and swept throughout all rooms',
      'Enabled the landlord to start painting and re-letting immediately'
    ]
  },
  {
    id: 'ba-3',
    category: 'Garage Clearance',
    title: 'De-cluttering for Workshop Conversion',
    description: 'A homeowner in Plympton needed to clear out a garage packed with old bikes, rusty tools, discarded car components, and unused gym equipment.',
    location: 'Plympton, Plymouth',
    beforeImageUrl: 'https://images.unsplash.com/photo-1595206133361-b1fe343e5e23?auto=format&fit=crop&w=800&q=80',
    afterImageUrl: 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80',
    details: [
      'Cleared obsolete paint tins (fully dried), broken timber workbenches, and rusty racking',
      'Extracted over 400kg of recyclable metal structures',
      'Swept concrete slab floor to high standard',
      'Sellers subsequently secured asking price on property listing'
    ]
  }
];
