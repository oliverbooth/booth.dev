const gulp = require("gulp");
const sass = require('gulp-sass')(require("sass"));
const cleanCSS = require("gulp-clean-css");
const rename = require("gulp-rename");
const ts = require("gulp-typescript");
const terser = require("gulp-terser");
const webpack = require("webpack-stream");
const path = require("path");
const named = require("vinyl-named");

const srcDir = "src";
const destDir = "OliverBooth/wwwroot";

function compileSCSS() {
    return gulp.src(`${srcDir}/scss/**/*.scss`)
        .pipe(sass().on("error", sass.logError))
        .pipe(cleanCSS({compatibility: "ie11"}))
        .pipe(rename({suffix: ".min"}))
        .pipe(gulp.dest(`${destDir}/css`));
}

function compileTS() {
    return gulp.src(`${srcDir}/ts/**/*.ts`)
        .pipe(ts("tsconfig.json"))
        .pipe(terser())
        .pipe(gulp.dest(`tmp/js`));
}

function bundleJS() {
    return gulp.src(["tmp/js/*.js", "tmp/js/app/app.js", "tmp/js/admin/admin.js"])
        .pipe(named())
        .pipe(webpack({ mode: "production", output: { filename: "[name].min.js" } }))
        .pipe(gulp.dest(`${destDir}/js`));
}

function copyJS() {
    return gulp.src(`${srcDir}/ts/**/*.js`)
        .pipe(rename({suffix: ".min"}))
        .pipe(gulp.dest(`${destDir}/js`));
}

function copyCSS() {
    return gulp.src(`${srcDir}/scss/**/*.css`)
        .pipe(rename({suffix: ".min"}))
        .pipe(gulp.dest(`${destDir}/css`));
}

function copyImages() {
    return gulp.src(`${srcDir}/img/**/*.*`)
        .pipe(gulp.dest(`${destDir}/img`));
}

exports.default = compileSCSS;
exports.default = gulp.parallel(compileSCSS, gulp.series(compileTS, bundleJS), copyCSS, copyJS, copyImages);
