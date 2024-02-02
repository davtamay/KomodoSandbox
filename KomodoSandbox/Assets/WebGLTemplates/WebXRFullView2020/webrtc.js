let localStream;
let peerConnection;
let socket;
let isMicrophoneMuted = false; // Tracks the microphone state
let isVideoShared = true; // Assuming video starts as shared

let isInCall = false;
let peerConfiguration = {
    iceServers: [
        {
            urls: [
                'stun:stun.l.google.com:19302',
                'stun:stun1.l.google.com:19302'
            ]
        }
    ]
}


//if(!RELAY_BASE_URL)
//let RELAY_BASE_URL = 'https://192.168.1.67:3000';

const constraints = {
    video: {
        width: { max: 256 },  // Maximum width
    //    height: { max: 192 }, // Maximum height
        aspectRatio: { ideal: 1.33 } // Optional: Ideal aspect ratio (16:9 in this case)
    },//true,
    audio: true
};
// Create a new MediaStream
var remoteStream = new MediaStream();
// Get the video element
var remoteVideoEl = document.getElementById('remoteVideo');
var localVideo = document.getElementById('localVideo');

document.getElementById('startButton').addEventListener('click', () => {
    navigator.mediaDevices.getUserMedia(constraints).then(stream => {
        localStream = stream;
        console.log(localStream);


        localVideo.srcObject = stream;


    }).catch(error => console.error('Error accessing media devices:', error));
});

function stopStream() {
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
    }
    if (peerConnection) {
        peerConnection.close();
    }
    if (socket) {
        socket.disconnect();
        socket = null;
    }
}

//let availableClients = [];

//let clientsOffered = []; // List of clients to whom an offer has been sent
let offers = []; // This should be your actual list of offers

//const userName = "Rob-" + Math.floor(Math.random() * 100000)
let userName = "Rob-" + Math.floor(Math.random() * 100000);
const password = "x";

// let offeredClients = new Set();
let didIOffer = false;

let isScreenSharing = false; 
//remove this when adding to unity
// document.addEventListener('DOMContentLoaded', () => {
//     ConnectToWebRTCSocket(0, "Rob-" + Math.floor(Math.random() * 100000));
// });

function ConnectToWebRTCSocket(id, name) {

    console.log(`ConnectToWebRTCSocket : ${name}`);

    userName = name;

    if (!socket) {
        socket = io(RELAY_BASE_URL, {
            auth: {
                userName, password
            }
        });

        setupSocketListeners();

        document.querySelector('#currentClientName').textContent = userName;
        // Set up socket event listeners
    }
}

document.getElementById('callButton').addEventListener('click', () => {
    if (!socket) {
        socket = io(RELAY_BASE_URL, {
            auth: {
                userName, password
            }
        });

        setupSocketListeners();
    }
    call();
});

let roomName = null;
let clientsInCall = [];

function setupSocketListeners() {

    socket.on('availableOffers', offers => {
        updateOfferElements(offers);
    });

    //is called for everyone just need to do if for the client that is being called
    socket.on('newOfferAwaiting', data => {

        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCall', data.receivingClientID);

           const name = data.newOffer.offererUserName;
           console.log(`newOfferAwaiting  NAME: ${name}`);
                const clientButton = document.getElementById(`call-${name}`);
                if (clientButton) {
                    clientButton.style.display = 'none';
                }
                
        isInCall = true;
        updateOfferElements(data.offers);
    });

   

    socket.on('roomCreated', (data) => {
        console.log(`roomCreated : ${data.roomName} with: ${data.nameToAdd}`);

        roomName = data.roomName;
       
        clientsInCall.push(data.nameToAdd);
    });

    socket.on('answerResponse', offerObj => {
        addAnswer(offerObj);
    });

    // socket.on('clientInCall', name => {

    //     answeredClient = name;
    //     //  answerClient(offerObj.offererUserName);
    //      // addAnswer(offerObj);
    //   });
  

    socket.on('receivedIceCandidateFromServer', iceCandidate => {
        addNewIceCandidate(iceCandidate);
    });
    socket.on('removeOffer', (offererUserName) => {
        console.log('removeOffer-------------', offererUserName);

        // Remove the offer from the client who answered the call
        offers = offers.filter(offer => offer.offererUserName !== offererUserName);
        updateOfferElements(offers); // Update the offer elements
    });

    socket.on('clientsUpdate', clients => {

        //availableClients = clients;
        console.log(`Available clients: ${clients}`);

        updateClientElements(clients);
    });

    socket.on('callEnded', () => {
        // Handle the call end event
        
        endAllConnections();
    });


    socket.on('clientDisconnected', (userName) => {
        // Handle the call end event
        
        clientDisconnected(userName);
    });


    

   

}

const endButton = document.getElementById('endButton');
endButton.addEventListener('click', endCall);

function endCall() {
    isInCall = false;
    clientsInCall.forEach(name => {

        const clientButton = document.getElementById(`call-${name}`);
        
        if (clientButton) clientButton.style.display = 'block';
        
       // document.getElementById(`call-${name}`).style.display = 'block';
    });


    console.log("roomname: " + roomName);
    socket.emit('sendCallEndedToServer', roomName);

    if (peerConnection) {
        peerConnection.close();
        //peerConnection = null;
        localStream.getTracks().forEach(track => track.stop());

        clientsInCall = [];
        
        // Reset the room name
        roomName = null;
    }
}

function endAllConnections() {

   // isInCall = false;
    clientsInCall.forEach(name => {
        document.getElementById(`call-${name}`).style.display = 'block';
       

        
     });

    if (peerConnection) {
        peerConnection.close();
       // peerConnection = null;
        localStream.getTracks().forEach(track => track.stop());

        clientsInCall = [];

        // Reset the room name
        roomName = null;
    }

}

function clientDisconnected(userName) {

        if(!isInCall)
        return;

        console.log(`clientDisconnected : ${clientsInCall}`);

        // Assuming 'name' is the username of the disconnected client
offers = offers.filter(offer => offer.offererUserName !== userName || offer.answererUserName !== userName);




          // Remove from clientsInCall
     clientsInCall = clientsInCall.filter(clientName => clientName !== userName);
    
        if (peerConnection) {
            peerConnection.close();
           // peerConnection = null;
           remoteStream.getTracks().forEach(track => track.stop());
           // localStream.getTracks().forEach(track => track.stop());
    
            clientsInCall = [];
    
            // Reset the room name
            roomName = null;
        } 

}




function updateClientElements(clients) {
    // Ensure that 'clients' is an array
    if (!Array.isArray(clients)) {
        console.error('Expected an array for clients, but received:', clients);
        return; // Exit the function if 'clients' is not an array
    }

    // Proceed with the rest of the function
    const clientsEl = document.getElementById('clients');
    if (clientsEl) {
        clientsEl.innerHTML = '';
        clients.forEach(client => {

            if (client !== userName) {
                const newClientEl = document.createElement('div');
                newClientEl.innerHTML = `<button id=call-${client} style="position: relative; z-index: 1000;" class="btn btn-success col-1">Call ${client}</button>`;
                newClientEl.addEventListener('click', () => {
                    call(client);
                });
                clientsEl.appendChild(newClientEl);
            }
            // Rest of the code to handle each client...
        });
    } else {
        console.error('Element with ID "clients" not found');
    }
}


function updateOfferElements(newOffers) {

     // Only keep the offers that are also in newOffers
     if(offers.length > 0)
     offers = offers.filter(offer => newOffers.some(newOffer => newOffer.offererUserName === offer.offererUserName));

    newOffers.forEach(newOffer => {

         // Skip if the offer is from the current client
         if (newOffer.offererUserName === userName) {
            return;
        }

        // Check if offer already exists
        const existingOfferIndex = offers.findIndex(offer => offer.offererUserName === newOffer.offererUserName);
        if (existingOfferIndex !== -1) {
            // Replace the existing offer with the new one
            offers[existingOfferIndex] = newOffer;
        } else {
            // Add new offer to the list
            offers.push(newOffer);
        }
    });


    const answerEl = document.querySelector('#answer');
    answerEl.innerHTML = '';
    offers.forEach(o => {
        const newOfferEl = document.createElement('div');
        newOfferEl.innerHTML = `<button id="${o.offererUserName}" class="btn btn-success col-1">Answer ${o.offererUserName}</button>`;
        newOfferEl.addEventListener('click', async () => {

            newOfferEl.parentElement.removeChild(newOfferEl);
            // Filter out the offers from the client who clicked the 'Answer' button
            offers = offers.filter(offer => offer.offererUserName !== o.offererUserName);
            socket.emit('offerAnswered', o.offererUserName); // Emit the offerer's username
            await answerOffer(o);

        });
        answerEl.appendChild(newOfferEl);
    });
}

async function answerClient(offererUserName) {
   
   // socket.emit('offerAnswered', offererUserName);
    offers.forEach(o => {
        offers = offers.filter(offer => offer.offererUserName !== o.offererUserName);
        socket.emit('offerAnswered', offererUserName); // Emit the offerer's username
         answerOffer(o);
    })
}


async function answerOffer(offerObj) {
    await fetchUserMedia();
    await createPeerConnection(offerObj);
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);

    offerObj.answer = answer;
    offerObj.answererUserName = userName;

    const offerIceCandidates = await socket.emitWithAck('newAnswer', offerObj);
    offerIceCandidates.forEach(c => {
      
  if (peerConnection && peerConnection.signalingState !== 'closed') 
        peerConnection.addIceCandidate(c);
    });
}

function addNewIceCandidate(iceCandidate) {
    if (peerConnection && peerConnection.signalingState !== 'closed') 
    peerConnection.addIceCandidate(new RTCIceCandidate(iceCandidate));
}

async function addAnswer(offerObj) {
    await peerConnection.setRemoteDescription(new RTCSessionDescription(offerObj.answer));
}

async function createPeerConnection(offerObj) {
    peerConnection = new RTCPeerConnection(peerConfiguration);
    remoteStream = new MediaStream();
    remoteVideoEl.srcObject = remoteStream;
    localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

    peerConnection.onicecandidate = e => {
        if (e.candidate) {
            socket.emit('sendIceCandidateToSignalingServer', {
                iceCandidate: e.candidate,
                iceUserName: userName,
                didIOffer
            });
        }
    };

    peerConnection.ontrack = e => {
        e.streams[0].getTracks().forEach(track => remoteStream.addTrack(track));
    };

    if (offerObj) {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(offerObj.offer));
    }
}
// Flag to indicate screen sharing state
async function fetchUserMedia(includeVideo = true) {
  
    const mediaConstraints = {
        audio: true,
        video: includeVideo ? constraints.video : false
    };

    try {
        originalStream = await navigator.mediaDevices.getUserMedia(mediaConstraints);
        localVideo.srcObject = originalStream;
        localStream = originalStream;
    } catch (videoError) {
        // if (videoError.name === "NotAllowedError" || videoError.name === "PermissionDeniedError") {
            // Video permission denied, try to get audio only
            try {
                const audioOnlyStream = await navigator.mediaDevices.getUserMedia({ audio: true });
                localStream = audioOnlyStream;
                // Note: No video element source for audio-only stream
                // Update UI accordingly, if necessary
            } catch (audioError) {
                console.error('Error accessing audio:', audioError);
                // Handle the case where neither video nor audio is accessible
                // Update UI to inform the user
            }
       
    }

//}
}


async function call(clientUserName) {
    console.log(`call : ${clientUserName}`);
    await fetchUserMedia();
    await createPeerConnection();

    try {
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        didIOffer = true;
        // Emitting the offer to the signaling server with details of the offerer, answerer, and any additional data required
        socket.emit('newOffer', { offer, offererUserName: userName, answererUserName: clientUserName, receivingClientID: window.client_id });
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}

document.getElementById('startScreenSharingButton').addEventListener('click',
async () => {
   // await fetchUserMedia(); // Refresh original stream
   document.getElementById('stopScreenSharingButton').removeAttribute('disabled');
   document.getElementById('startScreenSharingButton').setAttribute('disabled', false);
    await startScreenSharing();
}
);
document.getElementById('stopScreenSharingButton').addEventListener('click', ()=>{
    
    document.getElementById('stopScreenSharingButton').setAttribute('disabled',false);
    document.getElementById('startScreenSharingButton').removeAttribute('disabled');
    stopScreenSharing();
});


let screenStream;
let originalStream; // This will store the original user media stream

async function startScreenSharing() {
    if (!isScreenSharing) {
        try {
            screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            const screenTrack = screenStream.getVideoTracks()[0];
            const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
            if (sender) {
                await sender.replaceTrack(screenTrack);
            }
            localVideo.srcObject = screenStream;
            localStream = screenStream; // Update for signaling
            isScreenSharing = true;
        } catch (error) {
            console.error('Error starting screen sharing:', error);
        }
    }
}


async function stopScreenSharing() {
    // Stop screen sharing stream tracks
    if (screenStream) {
        screenStream.getTracks().forEach(track => track.stop());
    }

    // Check and replace track only if in screen sharing mode
    if (isScreenSharing && originalStream) {
        const videoTrack = originalStream.getVideoTracks()[0];
        const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
        if (sender && videoTrack) {
            await sender.replaceTrack(videoTrack);
            localVideo.srcObject = originalStream;
            localStream = originalStream; // Ensure localStream is set back to original
            console.log('Screen sharing stopped, and original video track restored.');
            isScreenSharing = false; // Reset screen sharing flag
        } else {
            console.error('Failed to find video sender or original video track.');
        }
    } else {
        console.error('No original stream available or not in screen sharing mode.');
    }

    // Refresh originalStream for future use
    await fetchUserMedia();
}

async function createAndSendOffer(remotePeerUserName) {
    try {
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        // Use the remotePeerUserName as part of the offer message
        socket.emit('newOffer', { offer, offererUserName: userName, answererUserName: remotePeerUserName, type: 'offer' });
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}

// document.getElementById('startAudioCallButton').addEventListener('click', () => {
//     fetchUserMedia(false); // Start an audio-only call
// });

document.getElementById('toggleMicrophoneButton').addEventListener('click', MuteMicToggle);

function MuteMicToggle(){

  // Update button text and color
//   if (isMicrophoneMuted) {
//     this.textContent = 'Mute Mic';

//     this.classList.add('unmuted');
//     this.classList.remove('muted');
    
// } else {
//     this.textContent = 'Unmute Mic';
   

//     this.classList.add('muted');
//     this.classList.remove('unmuted');
// }

// Assuming localStream is already acquired and includes audio tracks
if (localStream && localStream.getAudioTracks().length > 0) {
    isMicrophoneMuted = !isMicrophoneMuted; // Toggle the state
    localStream.getAudioTracks().forEach(track => track.enabled = !isMicrophoneMuted);
}
}

document.getElementById('toggleVideoButton').addEventListener('click', ShareVideoToggle);

function ShareVideoToggle()
{
     // Update button text and color
    //  if (isVideoShared) {
    //     this.textContent = 'Stop Sharing Video';
    //     this.classList.add('sharing');
    //     this.classList.remove('not-sharing');
    // } else {
    //     this.textContent = 'Share Video';
    //     this.classList.add('not-sharing');
    //     this.classList.remove('sharing');
    // }

       // Assuming localStream is already acquired and includes video tracks
       if (localStream && localStream.getVideoTracks().length > 0) {
        isVideoShared = !isVideoShared; // Toggle the state
        localStream.getVideoTracks().forEach(track => track.enabled = isVideoShared);
    }
}

