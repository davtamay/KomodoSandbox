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


        // Function called by Unity to change the video device
     ChangeVideoDevice: function(deviceId) {

        // Your logic to change the video device in WebRTC
        console.log("Changing video device to: ", UTF8ToString(deviceId));
        fetchUserMedia(null, UTF8ToString(deviceId))
        // Example: updateWebRTCVideoDevice(deviceId);
    },

    // Function called by Unity to change the audio device
     ChangeAudioDevice : function(deviceId) {
        // Your logic to change the audio device in WebRTC
        console.log("Changing audio device to: ", UTF8ToString(deviceId));
         fetchUserMedia(null, null, UTF8ToString(deviceId));
        // Example: updateWebRTCAudioDevice(deviceId);
    },

     ChangeAudioOutputDevice : function(deviceId) {
        // Your logic to change the audio device in WebRTC
        console.log("Changing audio output device to: ", UTF8ToString(deviceId));
        changeAudioOutputDevice(UTF8ToString(deviceId));
       //  fetchUserMedia(null, null, UTF8ToString(deviceId));
        // Example: updateWebRTCAudioDevice(deviceId);
    },



       RejectClientOffer: function(clientID) {
      
        rejectOffer(clientID);
      
       },


      RequestVideoReadyCheck : function(name, id) {
          console.log("RequestVideoReadyCheck : " + UTF8ToString(name));

          let nameConverted = UTF8ToString(name);
         
          if(nameConverted === "localVideo")
             checkVideoElementReady("localVideo", id);
          else
            checkVideoElementReady("remoteVideo_" + nameConverted, id);
      ///  window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveVideoReadyCheck', UTF8ToString(name));
       },


      RemoveWebRTCTexture: function(id, name) {
       RemoveWebRTCTextureJS(id, UTF8ToString(name));
      },




     SetupWebRTCTexture: function(id, name) {

      let textureName = UTF8ToString(name);

      let videoElement = null;
      
      if(textureName === "localVideo")
          videoElement = localVideo;
      else
         videoElement = document.getElementById("remoteVideo_" + textureName);
    


    //  videoElement.onloadedmetadata = function() {

    //     window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveDimensions', `${textureName},${videoElement.videoWidth},${videoElement.videoHeight}`);
    
    //  };


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
        //needed to keep track of the texture to update
        // videoElement.textureID = id;

// if(!videoElements.includes(videoElement)) {
//     videoElements.push(videoElement);
// }
         // if(videoElements.indexOf(videoElement) === -1) 
           //   videoElements.push(videoElement);
          
        
          console.log("WebGL texture created with ID:", id);

        // GLctx is the webgl context of the Unity canvas
        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);

        // Upload the canvas image to the GPU texture.
        GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);


 function texturePaint(v)
  {

        ctx.drawImage(v, 0, 0, 256, 256);

        GLctx.bindTexture(GLctx.TEXTURE_2D,  GL.textures[v.textureID]);//textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
    
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);

  }
 
  function updateTexture() 
  { 

    

    
    for(let v of videoElements)
    {

      if (v.readyState >= v.HAVE_CURRENT_DATA)
      { 
        console.log("video ready" + v.id + "VIDEOELEMENTS :"+ videoElements.length)
        texturePaint(v);
      }

    }
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

                for(let v of videoElements)
                {
                  if (v.readyState >= v.HAVE_CURRENT_DATA) 
                    texturePaint(v);
                };
          // if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) 
          //      texturePaint();

            func(time);
                
        });

      } 
      else {

         window.requestAnimationFrame(func);

      }
    };

        if (videoElements.length >= 0)
          updateTexture();
 

     }
  });
