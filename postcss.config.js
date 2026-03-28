const purgecss = require('@fullhuman/postcss-purgecss');
const cssnano = require('cssnano');

module.exports = {
  plugins: [
    purgecss({
      // Scan all Razor pages, layouts, JS files, and CS files that build class names
      content: [
        './Pages/**/*.cshtml',
        './wwwroot/js/**/*.js',
      ],
      // Bootstrap uses many dynamic classes added via JS — safelist them
      safelist: {
        standard: [
          // Theme & state classes added dynamically
          /^show$/,
          /^collapsing$/,
          /^collapsed$/,
          /^active$/,
          /^fade$/,
          /^in$/,
          // Bootstrap data-bs-theme system
          /^bs-/,
          /^data-bs-/,
          // Alert dismiss transition
          /^alert-dismissible$/,
          // Navbar states
          /^navbar-collapse$/,
          // Form validation states (for Newsletter pages)
          /^is-valid$/,
          /^is-invalid$/,
          /^valid-feedback$/,
          /^invalid-feedback$/,
          /^was-validated$/,
          // Dropdown positioning (added by Popper.js)
          /^bs-popover/,
          /^bs-tooltip/,
          /^dropdown-menu-end$/,
          /^dropup$/,
          /^dropend$/,
          /^dropstart$/,
        ],
        deep: [
          // Keep all rules targeting [data-bs-theme]
          /\[data-bs-theme/,
        ],
      },
      // Handle escaped characters in Bootstrap's CSS selectors
      defaultExtractor: content => {
        const broadMatches = content.match(/[^<>"'`\s]*[^<>"'`\s:]/g) || [];
        const innerMatches = content.match(/[^<>"'`\s.()]*[^<>"'`\s.():]/g) || [];
        return broadMatches.concat(innerMatches);
      },
    }),
    cssnano({
      preset: 'default',
    }),
  ],
};
