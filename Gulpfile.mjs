import fs from "fs";
import gulp from "gulp";
import cleanCSS from "gulp-clean-css";
import {deleteSync} from "del";
import noop from "gulp-noop";
import rename from "gulp-rename";
import gulpSass from "gulp-sass";
import * as nodeSass from "sass";
const sass = gulpSass(nodeSass);
import sourcemaps from "gulp-sourcemaps";
import ts from "gulp-typescript";
import webpack from "webpack-stream";
import vinylPaths from "vinyl-paths";

const srcDir = "src";
const tmpDir = "tmp";
const destDir = "OliverBooth/wwwroot";
const isDevelopment = !!process.env.DEVELOPMENT;

function cleanTMP() {
    return gulp.src(tmpDir, {allowEmpty: true})
        .pipe(vinylPaths(deleteSync));
}

function cleanWWWRoot() {
    return gulp.src(destDir, {allowEmpty: true})
        .pipe(vinylPaths(deleteSync));
}

function compileSCSS() {
    return gulp.src(`${srcDir}/scss/**/*.scss`)
        .pipe(isDevelopment ? sourcemaps.init() : noop())
        .pipe(sass().on("error", sass.logError))
        .pipe(cleanCSS({compatibility: "ie11"}))
        .pipe(rename({suffix: ".min"}))
        .pipe(isDevelopment ? sourcemaps.write() : noop())
        .pipe(gulp.dest(`${destDir}/css`));
}

function compileTS() {
    return gulp.src(`${srcDir}/ts/**/*.ts`)
        .pipe(isDevelopment ? sourcemaps.init() : noop())
        .pipe(ts("tsconfig.json"))
        .pipe(isDevelopment ? sourcemaps.write("./", { includeContent: true }) : noop())
        .pipe(gulp.dest(`${tmpDir}/js`));
}

function bundleJS(done) {
    const tasks = fs.readdirSync(`${tmpDir}/js`, {withFileTypes: true})
        .filter(dirent => dirent.isDirectory())
        .map(d => bundleDir(d.name));
    return gulp.parallel(...tasks, writeSourcemaps)(done);

    function bundleDir(directory) {
        return () => gulp.src(`${tmpDir}/js/${directory}/${directory}.js`)
            .pipe(isDevelopment ? sourcemaps.init() : noop())
            .pipe(webpack({mode: "production", output: {filename: `${directory}.min.js`}, devtool: "source-map"}))
            .pipe(isDevelopment ? sourcemaps.write("./", { includeContent: true }) : noop())
            .pipe(gulp.dest(`${destDir}/js`));
    }

    function writeSourcemaps() {
        return gulp.src(`${destDir}/js/**/*.js`)
            .pipe(isDevelopment ? sourcemaps.init({ loadMaps: true }) : noop())
            .pipe(isDevelopment ? sourcemaps.write("./", { includeContent: true }) : noop())
            .pipe(gulp.dest(`${destDir}/js`));
    }
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

function exists(path) {
    try {
        return fs.existsSync(path);
    } catch (err) {
        return false;
    }
}

gulp.task("clean", gulp.parallel(cleanTMP, cleanWWWRoot));
gulp.task("assets", copyImages);
gulp.task("styles", gulp.parallel(compileSCSS, copyCSS));
gulp.task("scripts", gulp.parallel(copyJS, gulp.series(compileTS, bundleJS)));
gulp.task("default", gulp.series("clean", gulp.parallel("styles", "scripts", "assets"), cleanTMP));
