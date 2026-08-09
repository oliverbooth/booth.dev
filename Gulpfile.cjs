const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const cleanCSS = require('gulp-clean-css');
const rename = require('gulp-rename');
const sourcemaps = require('gulp-sourcemaps');
const ts = require('gulp-typescript');
const terser = require('gulp-terser');
const webpack = require('webpack-stream');

const srcDir = 'src';
const destDir = 'BoothDotDev/wwwroot';

async function clean() {
    const {deleteAsync} = await import('del');
    return deleteAsync([`${destDir}/**/*`, `tmp/**/*`]);
}

function compileSCSS() {
    return gulp.src(`${srcDir}/scss/**/*.scss`)
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(cleanCSS({ compatibility: 'ie11' }))
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`${destDir}/css`));
}

function compileTS() {
    return gulp.src(`${srcDir}/ts/**/*.ts`)
        .pipe(sourcemaps.init())
        .pipe(ts("tsconfig.json"))
        .pipe(terser())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`tmp/js`));
}

function bundleJS() {
    return gulp.src('tmp/js/*.js', { sourcemaps: true })
        .pipe(webpack({
            mode: 'production',
            output: { filename: 'app.min.js' },
            devtool: 'source-map'
        }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`${destDir}/js`));
}

function copyJS() {
    return gulp.src(`${srcDir}/ts/**/*.js`)
        .pipe(sourcemaps.init())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`${destDir}/js`));
}

function copyCSS() {
    return gulp.src(`${srcDir}/scss/**/*.css`)
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`${destDir}/css`));
}

function copyImages() {
    return gulp.src(`${srcDir}/img/**/*.*`, { encoding: false })
        .pipe(sourcemaps.init())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(`${destDir}/img`));
}

exports.clean = clean;
exports.assets = copyImages;
exports.styles = gulp.parallel(compileSCSS, copyCSS);
exports.scripts = gulp.parallel(copyJS, gulp.series(compileTS, bundleJS));

exports.default = gulp.series(clean, gulp.parallel(exports.styles, exports.scripts, exports.assets));

exports.watch = function watch() {
    gulp.watch(`${srcDir}/scss/**/*.scss`, exports.styles);
    gulp.watch(`${srcDir}/ts/**/*.ts`, exports.scripts);
};
