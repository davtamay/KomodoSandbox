 mergeInto(LibraryManager.library, {

  GetVideoDimensions: function() {
     videoElement = document.getElementById('remoteVideo');
    // return [video.videoWidth, video.videoHeight];
     window.gameInstance.SendMessage('Plane', 'ReceiveDimensions', `${videoElement.videoWidth},${videoElement.videoHeight}`);
},

     UpdateCanvasTexture: function(id) {
        // Create and fill a canvas with a gradient
       ct_canvas = document.createElement("canvas");
        ct_canvas.width = 256;
        ct_canvas.height = 256;
       ctx = ct_canvas.getContext("2d");
        var grd = ctx.createLinearGradient(0, 0, 200, 0);
        grd.addColorStop(0, "red");
        grd.addColorStop(1, "white");
        ctx.fillStyle = grd;
        ctx.fillRect(0, 0, 256, 256);

        // Get the WebGL texture object from the Emscripten texture ID.
        textureObj = GL.textures[id];
textureID = id;

          console.log("WebGL texture created with ID:", id);

        // GLctx is the webgl context of the Unity canvas
        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);

        // Upload the canvas image to the GPU texture.
     //   GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);
GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);


  function updateTexture() {
              //   console.log("Texture is attemptiing to.");
   if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) {

  //does work. but zooms in 
      ctx.drawImage(videoElement, 0, 0, videoElement.videoWidth, videoElement.videoHeight);

        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
      
      
       GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);

        console.log("Texture updated with video frame." + "TextureID:" + textureID);
   }

    requestAnimationFrame(updateTexture);


    }

    updateTexture();
     }
//            CreateWebGLTexture: function (id) {
//         // Ensure the video element is available
//        videoElement = document.getElementById('remoteVideo');
//         if (!videoElement) {
//             console.error("Video element not found");
//             return 0;
//         }

//         // Create a canvas and WebGL context
//        canvas = document.createElement('canvas');
//         canvas.width = 256;
//         canvas.height = 256;
//          gl = canvas.getContext('webgl');


//       // Get the WebGL texture object from the Emscripten texture ID.
//        texture = GL.textures[id];

//           if (!texture) {
//             console.error("Texture not found for ID:", id);
//             return;
//         }

//         // Create and configure the WebGL texture
//        // var texture = gl.createTexture();
//         gl.bindTexture(gl.TEXTURE_2D, texture);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

//         // Assign the texture to a texture ID
//         // var textureId = GL.getNewId(GL.textures);
//         // GL.textures[textureId] = texture;

//         console.log("WebGL texture created with ID:", id);

//       function updateTexture() {
//               //   console.log("Texture is attemptiing to.");
//    if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) {
//       //  gl.bindTexture(gl.TEXTURE_2D, texture);
//         gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, videoElement);
//         console.log("Texture updated with video frame.");
//    }
//     requestAnimationFrame(updateTexture);
// }


//           updateTexture();

//   //      return textureId;
//     }






//         CreateWebGLTexture: function () {
//         // Ensure the video element is available
//         var videoElement = document.getElementById('remoteVideo');
//         if (!videoElement) {
//             console.error("Video element not found");
//             return 0;
//         }

//         // Create a canvas and WebGL context
//         var canvas = document.createElement('canvas');
//         canvas.width = 256;
//         canvas.height = 256;
//         var gl = canvas.getContext('webgl');

//         // Create and configure the WebGL texture
//         var texture = gl.createTexture();
//         gl.bindTexture(gl.TEXTURE_2D, texture);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
//         gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

//         // Assign the texture to a texture ID
//         var textureId = GL.getNewId(GL.textures);
//         GL.textures[textureId] = texture;

//         console.log("WebGL texture created with ID:", textureId);

//       function updateTexture() {
//               //   console.log("Texture is attemptiing to.");
//  //   if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) {
//         gl.bindTexture(gl.TEXTURE_2D, texture);
//         gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, videoElement);
//         console.log("Texture updated with video frame.");
//   //  }
//     requestAnimationFrame(updateTexture);
// }


//           updateTexture();

//         return textureId;
//     }

        });
