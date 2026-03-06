/**
 * Log Viewer — Minimal vanilla JS
 * Handles: dark mode toggle, keyboard shortcuts, row expand/collapse
 */
(function () {
  'use strict';

  const DARK_CLASS = 'lv-dark';
  const STORAGE_KEY = 'lv-dark-mode';

  // ---- Dark mode ----
  function applyDark(on) {
    const wrapper = document.getElementById('lv-app');
    if (!wrapper) return;
    wrapper.classList.toggle(DARK_CLASS, on);
  }

  function initDarkMode() {
    const saved = localStorage.getItem(STORAGE_KEY);
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const isDark = saved !== null ? saved === 'true' : prefersDark;
    applyDark(isDark);

    const btn = document.getElementById('lv-dark-toggle');
    if (btn) {
      btn.addEventListener('click', () => {
        const wrapper = document.getElementById('lv-app');
        const nowDark = wrapper ? !wrapper.classList.contains(DARK_CLASS) : false;
        applyDark(nowDark);
        localStorage.setItem(STORAGE_KEY, String(nowDark));
      });
    }
  }

  // ---- Row expand/collapse ----
  function initRowExpand(root) {
    root = root || document;
    root.querySelectorAll('.lv-row--expandable').forEach(function (row) {
      if (row.dataset.lvBound) return;
      row.dataset.lvBound = '1';

      function toggle() {
        const next = row.nextElementSibling;
        if (!next || !next.classList.contains('lv-exception-row')) return;
        const expanded = row.getAttribute('aria-expanded') === 'true';
        row.setAttribute('aria-expanded', String(!expanded));
        next.hidden = expanded;
      }

      row.addEventListener('click', toggle);
      row.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          toggle();
        }
      });
    });
  }

  // ---- Keyboard shortcuts ----
  function initKeyboard() {
    document.addEventListener('keydown', function (e) {
      // "/" — focus search
      if (e.key === '/' && document.activeElement !== document.getElementById('lv-search')) {
        const search = document.getElementById('lv-search');
        if (search) {
          e.preventDefault();
          search.focus();
          search.select();
        }
      }

      // Escape — clear search (only when search is focused)
      const search = document.getElementById('lv-search');
      if (e.key === 'Escape' && document.activeElement === search) {
        if (search && search.value) {
          search.value = '';
          search.dispatchEvent(new Event('keyup', { bubbles: true }));
        }
      }
    });
  }

  // ---- Sidebar active state ----
  function initSidebar() {
    document.querySelectorAll('.lv-file-link').forEach(function (link) {
      link.addEventListener('click', function () {
        document.querySelectorAll('.lv-file-item').forEach(function (item) {
          item.classList.remove('lv-file-item--active');
        });
        link.closest('.lv-file-item')?.classList.add('lv-file-item--active');
      });
    });
  }

  // ---- HTMX after-swap: re-bind row expand on new content ----
  document.addEventListener('htmx:afterSwap', function (e) {
    initRowExpand(e.detail.target);
  });

  // ---- Init ----
  document.addEventListener('DOMContentLoaded', function () {
    initDarkMode();
    initRowExpand();
    initKeyboard();
    initSidebar();
  });
})();
