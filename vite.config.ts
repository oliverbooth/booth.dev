import {defineConfig} from 'vite';
import {resolve} from 'path';

export default defineConfig({
    root: 'src',
    publicDir: '../public',
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
