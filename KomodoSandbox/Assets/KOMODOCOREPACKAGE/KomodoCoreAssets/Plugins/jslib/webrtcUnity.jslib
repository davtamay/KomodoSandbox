 mergeInto(LibraryManager.library, {



     ConnectToWebRTC: function(clientName) {
          
          console.log(clientName + " is connecting to WebRTC.");
          console.log(UTF8ToString(clientName) + "utf8 is connecting to WebRTC.");
          
          ConnectToWebRTCSocket( UTF8ToString(clientName));
        //window.sync.emit('connectToWebRTC', window.client_id, clientName);
     },

     CallClient: function(userName) {

        call(UTF8ToString(userName));
       
     },
    
     AnswerClientOffer: function(userName) {
      
       console.log(UTF8ToString(userName) + "utf8 answer");

        answerClient(UTF8ToString(userName));
         //  window.sync.emit('answerClient', userName);
     },

       HangUpClient: function() {
         endCall(1);
        // hangUp(UTF8ToString(userName));
       },

       ShareScreen: function(enabled) {
       
         if(enabled == 1)
         startScreenSharing();
         else
         stopScreenSharing();

       },

      

       SetMicrophone: function() {
         MuteMicToggle();
       },

        SetVideo: function() {
         ShareVideoToggle();
       },



     SetupWebRTCTexture: function(id) {

     videoElement = document.getElementById('remoteVideo');

     videoElement.onloadedmetadata = function() {

        window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveDimensions', `${videoElement.videoWidth},${videoElement.videoHeight}`);
    
     };


        // Create and fill a canvas with a gradient
       ct_canvas = document.createElement("canvas");
        ct_canvas.width =  256;
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
        GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);


 function texturePaint()
  {

        ctx.drawImage(videoElement, 0, 0, 256, 256);

        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
    
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);

  }
  function updateTexture(time, xrFrame) 
  {

      if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) 
        texturePaint();

       requestAnimationFrame(updateTexture);

  }


    //need to hijack this function for requestAnimationFrame to work during webxr session
    xrManager.BrowserObject.requestAnimationFrame = function (func) 
    {
          
      if (xrManager.xrSession && xrManager.xrSession.isInSession) 
      {
        return xrManager.xrSession.requestAnimationFrame((time, xrFrame) =>
        {
            xrManager.animate(xrFrame);

          if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) 
               texturePaint();

            func(time);
                
        });

      } 
      else {

         window.requestAnimationFrame(func);

      }
    };

 

    updateTexture();

     }
  });
