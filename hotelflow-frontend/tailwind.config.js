/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        display: ['"Playfair Display"', 'Georgia', 'serif'],
        body: ['"DM Sans"', 'system-ui', 'sans-serif'],
      },
      colors: {
        hotel: {
          cream: '#F8F3EC',
          gold: '#C8952A',
          'gold-light': '#E8B84B',
          navy: '#1A2744',
          'navy-dark': '#111827',
          slate: '#374151',
          muted: '#9CA3AF',
          border: '#E5DDD0',
          surface: '#FDFAF6',
        },
      },
      boxShadow: {
        card: '0 1px 3px rgba(0,0,0,0.06), 0 4px 16px rgba(0,0,0,0.06)',
        'card-hover': '0 4px 6px rgba(0,0,0,0.07), 0 12px 28px rgba(0,0,0,0.10)',
      },
    },
  },
  plugins: [],
}
