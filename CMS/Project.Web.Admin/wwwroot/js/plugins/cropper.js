"use strict";
var CropperInstance = function () {
    var chooseCropper;
    return {
        init: function (elementSelector, width = 500, height = 500, logBlobCallback) {
            const element = document.getElementById(elementSelector);
            let input = element.querySelector('[data-cropper="input"]');
            let image = element.querySelector('[data-cropper="modal-image"]');
            let thumbimage = element.querySelector('[data-cropper="thumbimage"]');
            let modalElement = element.querySelector('[data-cropper="image-modal"]');
            let boomodal = new bootstrap.Modal(modalElement);

            let zoominElement = element.querySelector('[data-cropper="zoomin"]');
            let zoomoutElement = element.querySelector('[data-cropper="zoomout"]');
            let cropElement = element.querySelector('[data-cropper="crop"]');

            input.addEventListener('change', function (e) {
                let files = e.target.files;
                let reader;
                let file;
                let url;
                let done = function (url) {
                    input.value = '';
                    image.src = url;
                    boomodal.show();
                };

                if (files && files.length > 0) {
                    file = files[0];

                    if (URL) {
                        done(URL.createObjectURL(file));
                    } else if (FileReader) {
                        reader = new FileReader();
                        reader.onload = function (e) {
                            done(reader.result);
                        };
                        reader.readAsDataURL(file);
                    }
                }
            });

            modalElement.addEventListener('shown.bs.modal', function (event) {
                chooseCropper = new Cropper(image, {
                    dragMode: 'move',
                    aspectRatio: width / height,
                    restore: false,
                    guides: false,
                    center: false,
                    highlight: false,
                    cropBoxMovable: false,
                    cropBoxResizable: false,
                    toggleDragModeOnDblclick: false,
                    minCropBoxHeight: 100,
                });
            });
            modalElement.addEventListener('hidden.bs.modal', function (event) {
                chooseCropper.destroy();
                chooseCropper = null;
            });

            thumbimage.addEventListener('click', function (e) {
                e.preventDefault();
                input.click();
            });
            zoominElement.addEventListener('click', function () {
                chooseCropper.zoom(0.1);
            });
            zoomoutElement.addEventListener('click', function () {
                chooseCropper.zoom(-0.1);
            });
            cropElement.addEventListener('click', function () {
                let canvas;
                boomodal.hide();
                if (chooseCropper) {
                    canvas = chooseCropper.getCroppedCanvas({
                        width: width,
                        height: height,
                    });
                    canvas.toBlob(function (blob) {
                        //chooseImageBolb = blob;
                        thumbimage.src = URL.createObjectURL(blob);

                        logBlobCallback(blob);
                    });
                }
            });
        }
    }
}();