/** @type {import('tailwindcss').Config} */

const azurePalettes = {
  // --- Główna paleta Azure ---
  'azure': {
    '0': '#000000',
    '10': '#001b3f',
    '20': '#002f65',
    '25': '#003a7a',
    '30': '#00458f',
    '35': '#0050a5',
    '40': '#005cbb',
    '50': '#0074e9',
    '60': '#438fff',
    '70': '#7cabff',
    '80': '#abc7ff',
    '90': '#d7e3ff',
    '95': '#ecf0ff',
    '98': '#f9f9ff',
    '99': '#fdfbff',
    '100': '#ffffff',
    'DEFAULT': '#0074e9', // alias dla '50'
  },

  // --- Paleta Secondary ---
  'azure-secondary': {
    '0': '#000000',
    '10': '#131c2b',
    '20': '#283041',
    '25': '#333c4d',
    '30': '#3e4759',
    '35': '#4a5365',
    '40': '#565e71',
    '50': '#6f778b',
    '60': '#8891a5',
    '70': '#a3abc0',
    '80': '#bec6dc',
    '90': '#dae2f9',
    '95': '#ecf0ff',
    '98': '#f9f9ff',
    '99': '#fdfbff',
    '100': '#ffffff',
    'DEFAULT': '#6f778b', // alias dla '50'
  },

  // --- Paleta Neutral ---
  'azure-neutral': {
    '0': '#000000',
    '4': '#0d0e11',
    '6': '#121316',
    '10': '#1a1b1f',
    '12': '#1f2022',
    '17': '#292a2c',
    '20': '#2f3033',
    '22': '#343537',
    '24': '#38393c',
    '25': '#3b3b3f',
    '30': '#46464a',
    '35': '#525256',
    '40': '#5e5e62',
    '50': '#77777a',
    '60': '#919094',
    '70': '#ababaf',
    '80': '#c7c6ca',
    '87': '#dbd9dd',
    '90': '#e3e2e6',
    '92': '#e9e7eb',
    '94': '#efedf0',
    '95': '#f2f0f4',
    '96': '#f4f3f6',
    '98': '#faf9fd',
    '99': '#fdfbff',
    '100': '#ffffff',
    'DEFAULT': '#77777a', // alias dla '50'
  },

  // --- Paleta Neutral Variant ---
  'azure-variant': {
    '0': '#000000',
    '10': '#181c22',
    '20': '#2d3038',
    '25': '#383b43',
    '30': '#44474e',
    '35': '#4f525a',
    '40': '#5b5e66',
    '50': '#74777f',
    '60': '#8e9099',
    '70': '#a9abb4',
    '80': '#c4c6d0',
    '90': '#e0e2ec',
    '95': '#eff0fa',
    '98': '#f9f9ff',
    '99': '#fdfbff',
    '100': '#ffffff',
    'DEFAULT': '#74777f', // alias dla '50'
  }
};

/**
 * Przekonwertowana paleta kolorów Blue do użycia w Tailwind CSS.
 * Używamy 'DEFAULT' dla poziomu 50.
 */
const bluePalettes = {
  // --- Główna paleta Blue (nazwana 'blue-primary' dla bezpieczeństwa) ---
  'blue-primary': {
    '0': '#000000',
    '10': '#00006e',
    '20': '#0001ac',
    '25': '#0001cd',
    '30': '#0000ef',
    '35': '#1a21ff',
    '40': '#343dff',
    '50': '#5a64ff',
    '60': '#7c84ff',
    '70': '#9da3ff',
    '80': '#bec2ff',
    '90': '#e0e0ff',
    '95': '#f1efff',
    '98': '#fbf8ff',
    '99': '#fffbff',
    '100': '#ffffff',
    'DEFAULT': '#5a64ff', // alias dla '50'
  },

  // --- Paleta Secondary ---
  'blue-secondary': {
    '0': '#000000',
    '10': '#191a2c',
    '20': '#2e2f42',
    '25': '#393a4d',
    '30': '#444559',
    '35': '#505165',
    '40': '#5c5d72',
    '50': '#75758b',
    '60': '#8f8fa6',
    '70': '#a9a9c1',
    '80': '#c5c4dd',
    '90': '#e1e0f9',
    '95': '#f1efff',
    '98': '#fbf8ff',
    '99': '#fffbff',
    '100': '#ffffff',
    'DEFAULT': '#75758b', // alias dla '50'
  },

  // --- Paleta Neutral ---
  'blue-neutral': {
    '0': '#000000',
    '4': '#0e0e11',
    '6': '#131316',
    '10': '#1b1b1f',
    '12': '#201f22',
    '17': '#2a292d',
    '20': '#303034',
    '22': '#353438',
    '24': '#3a393c',
    '25': '#3c3b3f',
    '30': '#47464a',
    '35': '#535256',
    '40': '#5f5e62',
    '50': '#78767a',
    '60': '#929094',
    '70': '#adaaaf',
    '80': '#c8c5ca',
    '87': '#dcd9dd',
    '90': '#e5e1e6',
    '92': '#ebe7eb',
    '94': '#f0edf1',
    '95': '#f3eff4',
    '96': '#f6f2f7',
    '98': '#fcf8fd',
    '99': '#fffbff',
    '100': '#ffffff',
    'DEFAULT': '#78767a', // alias dla '50'
  },

  // --- Paleta Neutral Variant ---
  'blue-variant': {
    '0': '#000000',
    '10': '#1b1b23',
    '20': '#303038',
    '25': '#3b3b43',
    '30': '#46464f',
    '35': '#52515b',
    '40': '#5e5d67',
    '50': '#777680',
    '60': '#91909a',
    '70': '#acaab4',
    '80': '#c7c5d0',
    '90': '#e4e1ec',
    '95': '#f2effa',
    '98': '#fbf8ff',
    '99': '#fffbff',
    '100': '#ffffff',
    'DEFAULT': '#777680', // alias dla '50'
  }
};

module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Rozpakowanie wszystkich czterech palet do konfiguracji
        ...azurePalettes,
        ...bluePalettes,
      },
    },
  },
  plugins: [],
}
