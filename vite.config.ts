import {defineConfig} from 'vite';
import {resolve} from 'path';

export default defineConfig({
    root: 'src',
    publicDir: '../public',
    build: {
        outDir: '../BoothDotDev/wwwroot',
        emptyOutDir: true,
        manifest: true,
        rollupOptions: {
            input: {
                app: resolve(__dirname, 'src/ts/app.ts'),
                style: resolve(__dirname, 'src/css/style.css'),
                prism_vs: resolve(__dirname, 'src/css/prism.vs.css'),
            },
        },
    },
});
