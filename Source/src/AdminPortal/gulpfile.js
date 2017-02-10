/// <binding ProjectOpened='default' />

// include plug-ins
var gulp = require('gulp'),
    watch = require("gulp-watch"),
    less = require('gulp-less'),
    cssmin = require('gulp-cssmin'),
    rename = require('gulp-rename'),
    plumber = require('gulp-plumber');

// files path
var path = {
    "main_less": "./wwwroot/less/main.less",
    "less": "./wwwroot/less/*.less",
    "css": "./wwwroot/css"
};

// Styles tasks
gulp.task('Styles', function () {
    return gulp.src(path.main_less)
        .pipe(plumber({
            errorHandler: function (err) {
                console.log(err);
                this.emit('end');
            }
        }))
        .pipe(less())
        .pipe(gulp.dest(path.css));
});

gulp.task('SetupFileWatch', function () {
    // Admin styles
    gulp.watch(path.less, ['Styles']);
});


gulp.task('default', ['SetupFileWatch']);