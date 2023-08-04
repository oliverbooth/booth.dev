const gulp = require('gulp');
const sass = require('gulp-sass')(require('node-sass'));
const cleanCSS = require('gulp-clean-css');
const rename = require('gulp-rename');

const srcDir = 'src/scss'; // Update with your SCSS source directory
const destDir = 'oliverbooth.dev/wwwroot/css'; // Update with your CSS output directory

function compileSCSS() {
    return gulp.src(`${srcDir}/**/*.scss`)
        .pipe(sass().on('error', sass.logError))
        .pipe(cleanCSS({ compatibility: 'ie11' }))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(destDir));
}

exports.default = compileSCSS;
