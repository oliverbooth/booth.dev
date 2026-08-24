import {defineConfig, type Plugin} from 'vite';
import {resolve} from 'path';
import {copyFileSync, mkdirSync, readdirSync} from 'fs';

/**
 * Copies Prism's per-language grammar files out of node_modules into the public dir.
 * @returns A Vite plugin that copies Prism language files to the public directory.
 */
function syncPrismComponents(): Plugin {
    return {
        name: 'sync-prism-components',
        // runs before Vite's own publicDir -> outDir copy, for both `vite` (dev) and `vite build`
        configResolved() {
            const srcDir = resolve(__dirname, 'node_modules/prismjs/components');
            const destDir = resolve(__dirname, 'public/js/prism-components');

            mkdirSync(destDir, {recursive: true});
            for (const file of readdirSync(srcDir)) {
                if (file.endsWith('.min.js')) {
                    copyFileSync(resolve(srcDir, file), resolve(destDir, file));
                }
            }
        },
    };
}

export default defineConfig({
    root: 'src',
    publicDir: '../public',
    plugins: [syncPrismComponents()],
    server: {
        port: 5173,
        strictPort: true,
    },
    build: {
        outDir: '../BoothDotDev/wwwroot',
        emptyOutDir: true,
        manifest: true,
        rollupOptions: {
            input: {
                app: resolve(__dirname, 'src/ts/app.ts'),
                prism_vs: resolve(__dirname, 'src/css/prism.vs.css'),
                style: resolve(__dirname, 'src/css/style.css'),

                admin: resolve(__dirname, 'src/ts/admin.ts'),
                admin_style: resolve(__dirname, 'src/css/admin.css'),
            },
        },
    },
});
