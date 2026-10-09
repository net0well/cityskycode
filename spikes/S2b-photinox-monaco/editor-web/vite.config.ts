import { defineConfig } from 'vite';

// Bundle 100% local (sem CDN), gerado direto na pasta servida pelo host.
export default defineConfig({
  base: './',
  build: {
    outDir: '../src/CitySky.Spike.EditorHost/wwwroot',
    emptyOutDir: true,
    target: 'es2022',
    chunkSizeWarningLimit: 6000,
  },
});
