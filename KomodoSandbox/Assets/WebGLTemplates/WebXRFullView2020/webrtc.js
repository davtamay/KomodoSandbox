let localStream;

let videoElements = [];//new Map();
let inactiveVideoElements = []; // Add this line to create the inactiveVideoElements list

let videoElementsMap = new Map();
//peer connection setup
let connectedPeers = new Set(); // Tracks peers we're connected to

let clientsInRoom = new Map();

let connectedPeersMap = new Map(); // Tracks peers we're connected to

let peerConnections = new Map();

let socket;
let isMicrophoneMuted = true; // Tracks the microphone state
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

let currentClientOffersMap = new Map();
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
var localVideo = document.getElementById('localVideo');

let roomName = null;
let clientsInCall = [];

let offers = []; // This should be your actual list of offers

//const userName = "Rob-" + Math.floor(Math.random() * 100000)
let userName = "Rob-" + Math.floor(Math.random() * 100000);
const password = "x";

// let offeredClients = new Set();
//let didIOffer = false;

let isScreenSharing = false;

//remove this when adding to unity
// document.addEventListener('DOMContentLoaded', () => {
//     ConnectToWebRTCSocket("Rob-" + Math.floor(Math.random() * 100000));
// });

function ConnectToWebRTCSocket(name) {

    console.log(`ConnectToWebRTCSocket : ${name}`);

    userName = name;

    if (!socket) {
        socket = io(RELAY_BASE_URL, {
            auth: {
                userName, password, client_id: window.client_id
            }
        });

        setupSocketListeners();

        document.querySelector('#currentClientName').textContent = userName;

    }
}

function setupSocketListeners() {

    socket.on('availableOffers', offers => {
        this.offers = offers;
        updateOfferElements(offers);
    });

    // socket.on('messageFromClient', (data, ackFn) => {
    //     console.log(`Received message: ${data.message} from ${data.from}`);

    //     // Process the message here

    //     // Send an acknowledgment back to the server, which will relay it to Client A
    //     if (ackFn) ackFn('Message received and processed');
    //   });
    //invoked on client that is being called
    socket.on('newOfferAwaiting', async (data, ackFn) => {

        let peerConnection = await getOrCreatePeerConnection(data.newOffer.offererUserName)//createPeerConnection(offer.offererUserName);
        await peerConnection.setRemoteDescription(new RTCSessionDescription(data.newOffer.offer));
        await processBufferedIceCandidates(data.newOffer.offererUserName, peerConnection);



        //let peerConnection = await getOrCreatePeerConnection(offer.offererUserName)//createPeerConnection(offer.offererUserName);


        console.log(`newOfferAwaiting : ${data.newOffer.offererUserName}`);

        currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);


        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCall', data.offererClientID);






        // await processBufferedIceCandidates(offer.offererUserName, peerConnection);



        addToOffers(data.newOffer);



        const responseObject = {
            status: 'success',
            message: `Call offer to ${data.newOffer.answererUserName} received and processed`,
            data: { /* some data */ }
        };


        if (ackFn) ackFn(responseObject);
    });


    socket.on('newOfferAwaiting2', async (data, ackFn) => {

        console.log(`newOfferAwaiting2 : ${data.newOffer.offererUserName}`);

        let peerConnection = await getOrCreatePeerConnection(data.newOffer.offererUserName)//createPeerConnection(offer.offererUserName);
        await peerConnection.setRemoteDescription(new RTCSessionDescription(data.newOffer.offer));
        await processBufferedIceCandidates(data.newOffer.offererUserName, peerConnection);

        currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);


        // setTimeout(async() => {
        await addToAutomaticClickedOffers({ offer: data.newOffer, offererSocketID: data.offererSocketID });

        // }, 1000);
        //addToOffers(data.newOffer);
        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCallAndAnswer', data.offererClientID);//addToOffers(data.newOffer);


        const responseObject = {
            status: 'success',
            message: `Call offer to ${data.newOffer.answererUserName} received and processed`,
            data: { /* some data */ }
        };


        if (ackFn) ackFn(responseObject);

        console.log(`RETURNING ACKFN : ${data.newOffer.offererUserName}`);

    }

    );


    socket.on('removeOffer', (offer) => {
        console.log('removeOffer-------------', offer);

        removeOffer(offer);
    });


    socket.on('roomCreated', (data) => {

        clientsInRoom.set(data.socketID, data.nameToAdd);

        roomName = data.roomName;

        clientsInCall.push(data.nameToAdd);
    });

    socket.on('answerResponse', async (data) => {

        let peerConnection = await getOrCreatePeerConnection(data.offer.answererUserName);//createPeerConnection(offer.answererUserName);

        if (connectedPeers.has(data.offer.answererUserName)) //connectedPeersMap.has(data.offer.answererUserName))
        {
            console.log(`This User: ${userName} is already connected to ${data.offer.answererUserName} aborting answerResponse.`);
            //cant do this after 4 clients there are empty ones
            //return;
        }



        console.log(`answerResponse : ${data.offer.answererUserName}`);


        await peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer.answer));
        await processBufferedIceCandidates(data.offer.answererUserName, peerConnection);




        // data.offer.answererIceCandidates.forEach(c => {

        //     if (peerConnection && peerConnection.signalingState !== 'closed')
        //         peerConnection.addIceCandidate(c);

        //     console.log(`${userName}  ======Added Ice Candidate======`)
        // });


        console.log(peerConnection.remoteDescription);
        //addAnswer(data.offer);

        // if (peerConnection.signalingState === 'stable') {
        //     console.log('Cant set Remote Pranser Description in stable state. Return.');
        //     return;
        // } else {
        //     await peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer.answer));
        //   //  console.log('The connection is not stable.');
        // }

        console.log(`CLIENT ID RECEIVING ANSWER RECEIPT : ${data.offererClientID}`);
        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', data.offererClientID);



        //if(!isForSync)
        if (connectedPeers.has(data.offer.answererUserName)) //connectedPeersMap.has(data.offer.answererUserName))
        {
            console.log(`This User: ${userName} is already connected to ${data.offer.answererUserName} aborting.`);
            //   return;
        } else {



            // connectedPeersMap.set(data.offer.offererUserName, data.offer.offererSocketID);

            connectedPeersMap.set(data.offer.answererUserName, data.offer.answererSocketID);

            if (!data.offer.isForSync) {
                socket.emit('roomCallClient', { clientToAdd: data.offer.offererUserName, clientsAlreadyConnectedTo: Array.from(connectedPeersMap.keys()) });
                console.log(`OFFERER CONNECTED PEERS:  ${connectedPeersMap.size})`)
            }

        }

        // socket.emit('connectionEstablished', {offererSocketID:offerResult.offererSocketID, answererSocketID: offerResult.answererSocketID, offererUserName: offerResult.offererUserName, answererUserName: offerResult.answererUserName, isForSync: isForSync });




    });




    socket.on('receivedIceCandidateFromServer', async (candidate) => {

        const peerConnection = await getOrCreatePeerConnection(candidate.from);//offer.answererUserName);

        if (candidate.iceCandidate && peerConnection) {

            if (!peerConnection.remoteDescription) {
                addIceCandidateToBuffer(candidate.from, candidate.iceCandidate);
                //   console.log(peerConnection.remoteDescription)
            } else {

                console.log(`receivedIceCandidateFromServer : ${candidate.from}`)

                peerConnection.addIceCandidate(new RTCIceCandidate(candidate.iceCandidate));
            }
        }
        else {
            console.log("Remote description not set. Buffering ICE candidate.");

            addIceCandidateToBuffer(candidate.from, candidate.iceCandidate);
        }

    });




    function addIceCandidateToBuffer(from, iceCandidate) {
        if (!iceCandidateBuffer.has(from)) {
            iceCandidateBuffer.set(from, []);
        }
        // Add the ICE candidate to the buffer
        iceCandidateBuffer.get(from).push(iceCandidate);
    }






    socket.on('clientsUpdate', clients => {

        //availableClients = clients;
        console.log(`Available clients: ${clients}`);

        updateClientElements(clients);
    });

    socket.on('addClient', clients => {

        //availableClients = clients;
        // console.log(`Available clients: ${clients}`);

        addClientElement(clients);
    });

   

    socket.on('callEnded', (clientData) => {
        // Handle the call end event

        if (window && window.gameInstance && window.socketIOAdapterName) {
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveCallEnded', clientData.clientID);
            console.log(`SENDMESSAGE RECEIVECALLENDED : ${clientData.clientName}`);
        }

        if (clientsCalled.has(clientData.clientName))
            clientsCalled.remove(clientData.clientName);

        endCall(0, clientData.clientName);

        console.log(`callEnded : ${clientData.clientName}`);
        //  endCall(0);


    });


    socket.on('callEndedAndEmptyRoom', () => {

        // console.log(`callEndedAndEmptyRoom :`);
        // // 1. Stop all local media tracks
        // if (localStream) {
        //     localStream.getTracks().forEach(track => track.stop());
        //     localStream = null; // Clear the localStream reference
        // }

        // // 2. Close all peer connections
        // peerConnections.forEach((peerConnection, key) => {
        //     peerConnection.close(); // Close the peer connection
        //     peerConnections.delete(key); // Remove from the map or collection
        // });



        // Handle the call end event

        if (window && window.gameInstance && window.socketIOAdapterName) {
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveEmptyRoom');
            console.log(`SENDMESSAGE RECEIVECALLENDED : ${userName}`);
        }


    });


    socket.on('clientDisconnected', (disconectingUserName) => {
        // Handle the call end event
        if (clientsCalled.has(disconectingUserName))
            clientsCalled.delete(disconectingUserName);

        endCall(0, disconectingUserName);
    });

    socket.on('acceptClientOffer', (data) => {

        acceptClientOffer(data);

    });

    socket.on('rejectedClientOffer', (data) => {

        if (clientsCalled.has(data.answererUserName))
            clientsCalled.delete(data.answererUserName);


            if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveCallRejected', data.answererClientID);

            
        console.log(`rejectedClientOffer : ${data}`);
        //  removeOffer(data);

    });

   


    socket.on('makeClientSendOffer', (clientToAdd) => {

        console.log(`makeClientSendOffer : ${clientToAdd}`);
        call(clientToAdd, true, true);
        //   call(clientToAdd, true);

    });


    socket.on('informAnswered', async (data) => {


        // let peerConnection = await getOrCreatePeerConnection(data.offererUserName, false);

        //attachIceCandidateListener(peerConnection, true);

        //     console.log(`inform of answer : ${data.offererUserName}`);
        //call(clientToAdd, true);
        //   call(clientToAdd, true);

    });
    // socket.on('syncForOfferer', (clientToAdd, offer) => {   

    //     console.log(`syncForOfferer : ${offer}`);
    //     connectedPeersMap.set(offer.offererUserName, offer.offererSocketID);
    //     connectedPeersMap.set(offer.answererUserName, offer.answererSocketID);
    //     socket.emit('roomCallClient', clientToAdd, Array.from(connectedPeersMap.values()));
    // });

    socket.on('messageFromClient', (data, ackFn) => {
        console.log(`Received message: ${data.message} from ${data.from}`);

        // Process the message here

        // Send an acknowledgment back to the server, which will relay it to Client A
        if (ackFn) ackFn('Message received and processed');
    });


}

const iceCandidateBuffer = new Map();
async function processBufferedIceCandidates(peerIdentifier, peerConnection) {
    if (iceCandidateBuffer.has(peerIdentifier)) {
        const candidates = iceCandidateBuffer.get(peerIdentifier);
        console.log(`Adding buffered ICE candidates for ${peerIdentifier}`);
        for (const candidate of candidates) {
            peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
        }
        // Clear the buffer for this peer connection
        iceCandidateBuffer.delete(peerIdentifier);
    }
}


// Set a maximum amount of time we're willing to wait for ICE to connect
const ICE_CONNECTION_TIMEOUT = 8000; // 15 seconds

function setupIceConnectionTimeout(peerConnection, peerId) {
    let iceConnectionTimer = setTimeout(() => {
        console.log(`ICE Connection Timeout. Restarting ICE for ${peerId}`);
        //   restartIce(peerConnection);
        call(peerId, true, true);
    }, ICE_CONNECTION_TIMEOUT);

    peerConnection.oniceconnectionstatechange = () => {
        console.log(`${peerId}: ICE connection state changed to: ${peerConnection.iceConnectionState}`);
        if (peerConnection.iceConnectionState === 'connected' ||
            peerConnection.iceConnectionState === 'completed') {
            clearTimeout(iceConnectionTimer);
        }
    };
}
//   async function restartIce(peerConnection, peerId) {
//       if (peerConnection.signalingState !== 'stable') {
//           console.log('Signaling state is not stable. Cannot restart ICE now.');
//           return; // Do not restart ICE if the signaling state is not stable
//       }

//       try {
//           const offer = await peerConnection.createOffer({ iceRestart: true });
//           await peerConnection.setLocalDescription(offer);

//           // Send the new offer to the signaling server
//           // Adjust this part based on how you send offers in your signaling logic
//           socket.emit('offer', {
//               type: 'ice-restart-offer',
//               offer: offer,
//               from: userName,
//               to: peerId
//           });

//           console.log(`ICE restart offer sent for ${peerId}`);
//       } catch (error) {
//           console.error(`Error during ICE restart for ${peerId}:`, error);
//       }
//   }




async function acceptClientOffer({ offer, offererClientID }) {

    //  if(!isAnswerer){
    socket.emit('offerAnswered', offer.offererUserName);
    //   socket.emit('offerAnswered', offer.answererUserName);

    console.log(`++++acceptClientOffer : ${offer.offererUserName}`);
    await answerOffer(offer);




    //if(offer.isAnswerer)
    if (window && window.gameInstance && window.socketIOAdapterName)
        window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', offererClientID);


}



function updateClientElements(clients) {

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

function addToOffers(offer) {

    // Update the DOM
    const answerEl = document.querySelector('#answer');
    if (answerEl) {
        const newOfferEl = document.createElement('div');
        newOfferEl.innerHTML = `<button id="${offer.offererUserName}" class="btn btn-success col-1">Answer ${offer.offererUserName}</button>`;
        newOfferEl.addEventListener('click', async () => {
            //socket.emit('offerAnswered', offer);
            console.log(`___________addToOffers : ${offer.offererUserName}`);
            await answerOffer(offer);
            newOfferEl.remove();
        });
        answerEl.appendChild(newOfferEl);
    } else {
        console.error('Element with id "answer" not found');
    }
}

async function addToAutomaticClickedOffers(data) {

    await answerOffer(data.offer, data.offererSocketID, true);
}

async function answerClient(offererUserName) {


    console.log(`answerClient (function parameter) : ${offererUserName}`);

  //  const valuesArray = Array.from(currentClientOffersMap.values());
    //console.log(valuesArray);


    const offer = currentClientOffersMap.get(offererUserName);

    console.log(`answerClient (existance in map) : ${offer}`);


  //  console.log(`answerClient client: ${currentClientOffersMap.size}`);

    await answerOffer(offer);


}

function removeOffer(offer) {

    if (!offer)
        return;

    // if(currentClientOffersMap.has(offer.offererUserName))
    // currentClientOffersMap.delete(offer.offererUserName);

    const offerEl = document.getElementById(offer.offererUserName);


    if (offerEl) {
        offerEl.remove();
    } else {
        console.warn('Element with id not found');
    }

}

const endButton = document.getElementById('endButton');
endButton.addEventListener('click', () => endCall(1));

// window.addEventListener('beforeunload', (event) => {
//     endCall(1);
//   });

async function endCall(isCaller, disconnectingUserName) {

    //clientsInCall contais the names of the clients alternative to the user. Answerer -> Offerer / Offerer -> Answeret
    clientsInCall.forEach(name => {

        const clientButton = document.getElementById(`call-${name}`);

        if (clientButton) clientButton.style.display = 'block';

    });


    if (disconnectingUserName) {

        clientsInCall = clientsInCall.filter(client => client !== disconnectingUserName);


        let peerConnection = peerConnections.get(disconnectingUserName);

        console.log(`endCall : ${disconnectingUserName} peerConnection: ${peerConnection}`)
        if (peerConnection) {
            peerConnection.close();
            peerConnections.delete(disconnectingUserName);
            peerConnection = null;

            let videoElementId = "remoteVideo_" + disconnectingUserName;
            let videoElement = document.getElementById(videoElementId);

            if (videoElement)
                videoElement.parentNode.removeChild(videoElement);

        }

    }


    if (isCaller) {

        localStream.getTracks().forEach(track => track.stop());
        clientsInCall = [];
        roomName = null;


        // Close all peer connections
        peerConnections.forEach((peerConnection, key) => {
            if (peerConnection) {
                peerConnection.close();
                peerConnections.delete(key);
            }
        });


        //  console.log("roomname: " + roomName);
        socket.emit('sendCallEndedToServer', userName);
    }

}


function clientDisconnected(userName) {

    console.log(`clientDisconnected : ${clientsInCall}`);

    endCall(0, userName);
}

function rejectOffer(clientID) {
    // Construct a rejection message
    const rejectionMessage = {
        type: 'offer-rejection',


        //offererUserName: offererUserName,
        
        

        // offererSocketID: offererSocketID,
        offererClientID: clientID,
        answererUserName: userName,
        reason: 'rejectedOffer' // Optional, you can provide a reason
    };

    // Send the rejection message to the signaling server
    socket.emit('requestRejectOffer', rejectionMessage);
}






function updateOfferElements(newOffers) {

    // Filter and update the offers
    //offers = offers.filter(offer => newOffers.some(newOffer => newOffer.offererUserName === offer.offererUserName));

    newOffers.forEach(newOffer => {
        if (newOffer.offererUserName !== userName) {
            const existingOfferIndex = offers.findIndex(offer => offer.offererUserName === newOffer.offererUserName);
            if (existingOfferIndex !== -1) {
                offers[existingOfferIndex] = newOffer;
            } else {
                offers.push(newOffer);
            }
        }
    });

    // Update the DOM
    const answerEl = document.querySelector('#answer');
    if (answerEl) {
        answerEl.innerHTML = '';
        offers.forEach(o => {
            const newOfferEl = document.createElement('div');
            newOfferEl.innerHTML = `<button id="${o.offererUserName}" class="btn btn-success col-1">Answer ${o.offererUserName}</button>`;
            newOfferEl.addEventListener('click', async () => {
                newOfferEl.remove();
                //  offers = offers.filter(offer => offer.offererUserName !== o.offererUserName);
                socket.emit('offerAnswered', o.offererUserName);
                await answerOffer(o);
            });
            answerEl.appendChild(newOfferEl);
        });
    } else {
        console.error('Element with id "answer" not found');
    }
}


//let currentOffererUserName;

async function answerOffer(offer, offererSocketID, isForSync) {


    //call this for the offerer socket aswell getting to offerer as someone needed to be exposed. this is done
    //since the sole offerer that does not know about peers is the one that is calling the answerer 
    //with multi peers



    console.log(`answerOffer : ${offer}`);

    let peerConnection = await getOrCreatePeerConnection(offer.offererUserName)//createPeerConnection(offer.offererUserName);
    // await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.offer));
    //  await processBufferedIceCandidates(offer.offererUserName, peerConnection);


    // offer.offerIceCandidates.forEach(c => {

    //     if (peerConnection) //&& peerConnection.signalingState !== 'closed')
    //         peerConnection.addIceCandidate(c);

    //     console.log(`${userName}  ======Added Ice Candidate======`)
    // });

    console.log(`POST PROCESS BUFFER`);

    const answer = await peerConnection.createAnswer({});

    await peerConnection.setLocalDescription(answer);

    // await waitForIceGatheringComplete(peerConnection);


    console.log(peerConnection.remoteDescription);


    offer.answer = answer;
    // offer.answererUserName = userName;
    offer.isForSync = isForSync;

    let offerResult = null;





    // offer.clientsInCall = clientsInCall;
    //  socket.emit('informClientOfAnswer', { offererSocketID:offer.offererSocketID, answererSocketID: offer.answererSocketID, offererUserName: offer.offererUserName, answererUserName: userName, isForSync: isForSync });

    //let data= {offer, clientsInRoom};
    offerResult = await socket.emitWithAck('newAnswer', { offer, clients: clientsInRoom.values() });



    // await processBufferedIceCandidates(offer.offererUserName, peerConnection);



    console.log(`ANSWERER CONNECTED PEERS:  ${connectedPeersMap.size})`)
    //if(offererSocketID)

    //if(!isForSync)
    if (connectedPeers.has(offerResult.offererUserName)) //connectedPeersMap.has(offerResult.offererUserName))
    {
        console.log(`This User: ${userName} is already connected to ${offerResult.offererUserName} in answerOffer answer.`);
        //  return;
    } else {


        //  setTimeout(() => {  
        connectedPeersMap.set(offer.offererUserName, offer.offererSocketID);
        //  connectedPeersMap.set(offer.answererUserName, offer.answererSocketID);

        if (!isForSync) {
            console.log(`ANSWER OFFERER :  ${offer.offererUserName} OFFFER RESULT: ${offerResult.offererUserName} CONNECTED PEERS:  ${connectedPeersMap.size})`)
            socket.emit('roomCallClient', { clientToAdd: offer.answererUserName, clientsAlreadyConnectedTo: Array.from(connectedPeersMap.keys()) });

        }
        //{clientToAdd: data.offer.offererUserName, clientsAlreadyConnectedTo:  Array.from(connectedPeersMap.values())}

        //   }, 1000);

    }

    //ReceiveClientAnswer   socket.emit('connectionEstablished', {offererSocketID:offerResult.offererSocketID, answererSocketID: offerResult.answererSocketID, offererUserName: offerResult.offererUserName, answererUserName: offerResult.answererUserName, isForSync: isForSync });


}


async function getOrCreatePeerConnection(name, didIOffer = false, isForClientSync = false) {

    let peerConnection;

    //syncing more than 2 users per answered client required overwriting the peer connection.
    //need to look for a better approach in the future

    peerConnection = peerConnections.get(name);

    if (!peerConnection) {
        console.log(`CreatePeerConnection : ${name}`);

        peerConnection = await createPeerConnection(name, didIOffer);


        if (isForClientSync)
            // Set up ICE connection state monitoring with a timeout
            setupIceConnectionTimeout(peerConnection, name);


        attachIceCandidateListener(peerConnection, didIOffer);
        listenAndSetupRemoteVideoStream(peerConnection, name);
        peerConnections.set(name, peerConnection);
        await fetchUserMedia(peerConnection);







    }




    return peerConnection;

}

function sendLocalFeedToRemotePeers() {

    peerConnections.forEach((peerConnection, peerId) => {
        // Remove all tracks from the peer connection
        peerConnection.getSenders().forEach(sender => peerConnection.removeTrack(sender));

        // Add the new tracks to the peer connection
        //localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
    });

    localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));


}


function listenAndSetupRemoteVideoStream(peerConnection, name) {


    // Handle remote track
    peerConnection.ontrack = event => {
        const remoteStream = new MediaStream();
        event.streams[0].getTracks().forEach(track => {

            remoteStream.addTrack(track)
            // console.log(`Track state: ${track.readyState}, enabled: ${track.enabled}`);

        }


        );

        // Update or create remote video element for the target user
        if (event.track.kind === 'video') {
            const videoElement = updateRemoteVideoElement(name);

            videoElement.srcObject = remoteStream;
            // Play the video element when it's ready
            videoElement.onloadedmetadata = () => {

                if (!connectedPeers.has(name)) {
                    connectedPeers.add(name);
                    console.log(`Peer ${name} added to connected peers.`);
                }


                console.log(`onloadedmetadata: Video is now ready to play for ${name}`);
                playVideoSafely(videoElement); // Function to safely play the video
            };

            videoElement.oncanplaythrough = () => {
                console.log("Can play through video now. Attempting to play...");
                playVideoSafely(videoElement);
            };

            console.log(`Remote stream added for ${name}: "elementname: ${videoElement.id} : ${remoteStream}`);
        }
        console.log("Track received:", event.track.kind, event.track.id);
        console.log("Associated stream IDs:", event.streams.map(s => s.id));




    };

}
function playVideoSafely(videoElement) {
    videoElement.play().then(() => {
        console.log("Playback started successfully.");
    }).catch(error => {
        console.error("Error attempting to play video:", error);
        // Handle autoplay restrictions or other playback issues here
        // For example, you might show a UI element to let users manually start playback
    });
}

async function createPeerConnection(targetUserName, didIOffer = false) {
    let peerConnection = new RTCPeerConnection(peerConfiguration);


    peerConnection.oniceconnectionstatechange = () => {

        console.log(`${targetUserName} : ICE connection state changed to: ${peerConnection.iceConnectionState}`);
        // peerConnection.iceConnectionState
        checkAndAddPeer(targetUserName, peerConnection);

        if (peerConnection.iceConnectionState === 'disconnected' || peerConnection.iceConnectionState === 'failed') {
            // Handle disconnection or failure
            if (connectedPeers.has(targetUserName)) {
                connectedPeers.delete(targetUserName);
                console.log(`Peer ${targetUserName} disconnected or failed to connect. Retrying...`);
                call(targetUserName, true);
            }
        }
    };

    peerConnection.onicegatheringstatechange = function () {
        console.log(`${targetUserName} : ICE gathering state changed to: ' ${peerConnection.iceGatheringState}`);

        // switch(peerConnection.iceGatheringState) {
        //     case 'new':
        //         // The ICE agent is gathering addresses or is waiting to be given remote candidates
        //         // through calls to addIceCandidate (or both).
        //         break;
        //     case 'gathering':
        //         // The ICE agent is actively gathering candidates.
        //         break;
        //     case 'complete':
        //         // The ICE agent has finished gathering candidates.
        //         break;
        // }
    };


    peerConnection.onconnectionstatechange = () => {

        console.log(`${targetUserName} : Connection state changed to: ${peerConnection.connectionState}`);

        checkAndAddPeer(targetUserName, peerConnection);


        if (peerConnection.connectionState === 'disconnected' || peerConnection.connectionState === 'failed') {
            // Handle disconnection or failure
            if (connectedPeers.has(targetUserName)) {
                connectedPeers.delete(targetUserName);
                console.log(`Peer ${targetUserName} disconnected or failed to connect. Retrying...`);
                call(targetUserName, true);
            }
        }

    };

    peerConnection.onsignalingstatechange = () => {

        console.log(`${targetUserName} : Signaling state changed to: ${peerConnection.signalingState}`);
        checkAndAddPeer(targetUserName, peerConnection);
    };






    return peerConnection;
}

function checkAndAddPeer(peerId, peerConnection) {
    if (peerConnection.iceConnectionState === 'connected' || peerConnection.iceConnectionState === 'completed') {
        if (peerConnection.connectionState === 'connected') {
            if (peerConnection.signalingState === 'stable') {

                if (!connectedPeers.has(peerId))
                    connectedPeers.add(peerId);
                console.log(`Peer ${peerId} added to connected peers.`);

                let videoEl = videoElementsMap.get(peerId);

                // Check if the video is playing and has enough data
                if (videoEl && !videoEl.paused && videoEl.readyState === 4) {
                    // All conditions are met, add the peer to the set
                }
            }
        }
    }
}

// Function to update or create a new remote video element for a given userName
function updateRemoteVideoElement(userName) {

    console.log(`updateRemoteVideoElement : ` + "remoteVideo_" + userName);
    let videoEl = document.getElementById(`remoteVideo_${userName}`);

    // if (!videoEl) {
    //     // If no existing video element is found, try to get an inactive video element from the pool
    //     if (inactiveVideoElements.length > 0) {
    //         videoEl = inactiveVideoElements.pop();
    //         videoEl.id = "remoteVideo_" + userName;
    //     }
    // }


    if (!videoEl) {
       console.log(`created __ updateRemoteVideoElement : ` + "remoteVideo_" + userName);
        videoEl = document.createElement('video');
        videoEl.id = `remoteVideo_${userName}`;
        videoEl.autoplay = true;
        videoEl.playsInline = true;
      //  videoEl.muted = true; // Add playsInline for compatibility with mobile browsers

        // Create a new div element for the label
        let labelEl = document.createElement('div');
        labelEl.innerText = userName;

        // Create a new div element for the container
        let containerEl = document.createElement('div');
        containerEl.style.display = 'flex';
        containerEl.style.flexDirection = 'column';
        containerEl.style.alignItems = 'center';
        containerEl.appendChild(videoEl);
        containerEl.appendChild(labelEl);

        // Append the container element to the body
        document.body.appendChild(containerEl);



        // Set CSS properties to make the video element hidden and non-interactive
        //videoEl.style.position = 'absolute';
        // videoEl.style.visibility = 'hidden';
        videoEl.style.pointerEvents = 'none';


        //  document.body.appendChild(videoEl); // Or append to a specific container
        
        videoElementsMap.set(userName, videoEl);
        // videoIndex++;

      //  unityInstance.SendMessage('WebRTCVideo', 'OnVideoReady', userName);
        
        

        // videoEl.onloadedmetadata = function() {

        //     window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveDimensions', `${userName},${videoEl.videoWidth},${videoEl.videoHeight}`);
    
        // };

    
    
    }

    //if(!videoElements.includes(videoEl))
            videoElements.push(videoEl);

    // videoEl.srcObject = stream;

    return videoEl;

}
function attachIceCandidateListener(peerConnection, didIOffer) {
    peerConnection.onicecandidate = e => {
        // peerConnection.addEventListener('icecandidate', e => {
        //  console.log('ICE candidate found:', e);
        if (e.candidate) {
            socket.emit('sendIceCandidateToSignalingServer', {
                iceCandidate: e.candidate,
                iceUserName: userName,
                didIOffer,
            });
        }
    };
}






// Flag to indicate screen sharing state
async function fetchUserMedia(peerConnection, includeVideo = true) {

    const mediaConstraints = {
        audio: true,
        video: includeVideo ? constraints.video : false
    };

    try {
        originalStream = await navigator.mediaDevices.getUserMedia(mediaConstraints);
        localVideo.srcObject = originalStream;
        localStream = originalStream;


        if(videoElements.includes(localVideo) === false)
                videoElements.push(localVideo);


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

    localStream.getTracks().forEach(track => {
        console.log(`ADDED TRACK: ${track.enabled}`);
        peerConnection.addTrack(track, localStream)
    }

    );

}

const callTimeoutDuration = 5000; // Timeout duration in milliseconds

let clientsCalled = new Set();
async function call(sendToUserName, isForClientSync = false, restartIce = false) {

    // if (clientsCalled.has(sendToUserName)) {
    //     console.log(`This User: ${userName} is already calling ${sendToUserName} aborting call.`);
    //     return;
    // }

    // clientsCalled.add(sendToUserName);


    if (sendToUserName === userName) {

        console.log(`Calling yourself: ${userName} `);
        return;
    }
    // if(connectedPeers.has(sendToUserName)) //connectedPeersMap.has(sendToUserName))
    // {
    //     console.log(`This User: ${userName} is already connected to ${sendToUserName} aborting call.`);
    //     return;
    // }
    let peerConnection;

    if (restartIce)
        peerConnection = await getOrCreatePeerConnection(sendToUserName, true, true);
    else
        peerConnection = await getOrCreatePeerConnection(sendToUserName, true, false);

    try {



        // peerConnection.createOffer().then(offer => {
        //     return peerConnection.setLocalDescription(offer);
        // }).then(() => {
        //     return waitForIceGatheringComplete(peerConnection);
        // }).then(() => {
        //     const offer = peerConnection.localDescription;
        //     enqueueSignalingMessage({
        //         type: 'offer',
        //         offer: offer,
        //         target: targetPeerId
        //     });
        // }).catch(error => {
        //     console.error("Error creating or sending offer:", error);
        // });







        let offer = null;

        if (restartIce)
            offer = await peerConnection.createOffer({ iceRestart: true });
        else
            offer = await peerConnection.createOffer();

        await peerConnection.setLocalDescription(offer);

        await waitForIceGatheringComplete(peerConnection);

        console.log(`CCCCCalling ${sendToUserName}. The local peer has created an offer and set it in the local description.`);


        // let adjustedAnswer;
        // switch (peerConnection.signalingState) {
        //     case 'stable':
        //         adjustedAnswer = `Calling ${sendToUserName}. The connection is stable.`;
        //         break;
        //     case 'have-local-offer':
        //         adjustedAnswer = `Calling ${sendToUserName}. The local peer has created an offer and set it in the local description.`;
        //         break;
        //     case 'have-remote-offer':
        //         adjustedAnswer = `Calling ${sendToUserName}. The remote peer has set the offer in the Remote description.`;
        //         break;
        //     case 'have-local-pranswer':
        //         adjustedAnswer = `Calling ${sendToUserName}. The local peer has created a provisional answer and set it in the local desciption.`;
        //         break;
        //     case 'have-remote-pranswer':
        //         adjustedAnswer = `Calling ${sendToUserName}. The remote peer has created a provisional answer and set it in the remote desciption.`;
        //         break;
        //     case 'closed':
        //         adjustedAnswer = `Calling ${sendToUserName}. The connection is closed.`;
        //         break;
        //     default:
        //         console.error('Unknown signaling state:', peerConnection.signalingState);
        //         return;
        // }

        // // Log the adjusted answer
        // console.log("+++++++++++++++" + adjustedAnswer);


        let ackReceived = false; // Flag to track acknowledgment receipt


        // Emitting the offer to the signaling server with details about the target user
        socket.emit('newOffer', {
            offer,
            offererUserName: userName,
            answererUserName: sendToUserName,
            isForClientSync
        },
            (response) => {

                ackReceived = true; // Acknowledgment received

                //console.log('Acknowledgment from Client B:', response)
                if (response.status === 'success') {

                    console.log('Message:', response.message);
                    // Process the response data
                } else {
                    // Handle errors or unsuccessful responses
                }

            });

        // Setting a timeout to check for acknowledgment
        setTimeout(() => {
            if (!ackReceived) {
                console.error('Message delivery failed or acknowledgment was not received within the timeout period.');
                // Handle the delivery failure (e.g., retry sending the message, alert the user)
            } else {
                if (clientsCalled.has(sendToUserName)){


                    if (window && window.gameInstance && window.socketIOAdapterName)
                    window.gameInstance.SendMessage(window.socketIOAdapterName, 'CallFailed', sendToUserName);
        
                            




                    clientsCalled.delete(sendToUserName);

                }
            }
        }, callTimeoutDuration);



    } catch (error) {
        console.error('Error creating offer:', error);
    }
}



function createOfferAndSend(peerConnection, targetPeerId) {
    peerConnection.createOffer().then(offer => {
        return peerConnection.setLocalDescription(offer);
    }).then(() => {
        return waitForIceGatheringComplete(peerConnection);
    }).then(() => {
        const offer = peerConnection.localDescription;
        enqueueSignalingMessage({
            type: 'offer',
            offer: offer,
            target: targetPeerId
        });
    }).catch(error => {
        console.error("Error creating or sending offer:", error);
    });
}

function waitForIceGatheringComplete(peerConnection) {
    return new Promise((resolve) => {
        if (peerConnection.iceGatheringState === "gathering") {
            resolve();
        } else {
            const checkState = () => {
                if (peerConnection.iceGatheringState === "gathering") {
                    peerConnection.removeEventListener("icegatheringstatechange", checkState);
                    resolve();
                }
            };
            peerConnection.addEventListener("icegatheringstatechange", checkState);
        }
    });
}



document.getElementById('startScreenSharingButton').addEventListener('click',
    async () => {
        // await fetchUserMedia(); // Refresh original stream
        document.getElementById('stopScreenSharingButton').removeAttribute('disabled');
        document.getElementById('startScreenSharingButton').setAttribute('disabled', false);
        await startScreenSharing();
    }
);
document.getElementById('stopScreenSharingButton').addEventListener('click', () => {

    document.getElementById('stopScreenSharingButton').setAttribute('disabled', false);
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

            screenTrack.onended = stopScreenSharing;
            // screenStream.getTracks().forEach(track => {
            //     track.onended = stopScreenSharing;
            // });


            clientsInCall.forEach(async (name) => {

                let peerConnection = peerConnections.get(name);
                const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
                if (sender) {
                    await sender.replaceTrack(screenTrack);
                }

            });


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

        clientsInCall.forEach(async (name) => {
            let peerConnection = peerConnections.get(name);
            const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
            if (sender && videoTrack) {
                await sender.replaceTrack(videoTrack);
            }
            else
                console.error('Failed to find video sender or original video track.');
        });

        localVideo.srcObject = originalStream;
        localStream = originalStream; // Ensure localStream is set back to original
        console.log('Screen sharing stopped, and original video track restored.');
        isScreenSharing = false; // Reset screen sharing flag
    }
    else {
        console.error('No original stream available or not in screen sharing mode.');
    }

    // Refresh originalStream for future use
    //  await fetchUserMedia();
}

document.getElementById('toggleMicrophoneButton').addEventListener('click', MuteMicToggle);

function MuteMicToggle() {

    isMicrophoneMuted = !isMicrophoneMuted; // Toggle the state

    peerConnections.forEach(peerConnection => {
        const senders = peerConnection.getSenders();
        senders.forEach(sender => {
            if (sender.track && sender.track.kind === 'audio') {
                sender.track.enabled = isMicrophoneMuted;
            }
        });
    });
}

document.getElementById('toggleVideoButton').addEventListener('click', ShareVideoToggle);

async function ShareVideoToggle() {
    isVideoShared = !isVideoShared;

    peerConnections.forEach(peerConnection => {
        const senders = peerConnection.getSenders();
        senders.forEach(sender => {
            if (sender.track && sender.track.kind === 'video') {
                sender.track.enabled = isVideoShared;
            }
        });
    });



}




function RemoveWebRTCTextureJS(id, name) {
    // let textureObj = GL.textures[id];
    // GLctx.deleteTexture(textureObj);
    // console.log("WebGL texture deleted with ID:", id);

   // let videoElement = document.getElementById("remoteVideo_" + name);

    let videoElement = undefined

    if(name === "localVideo")
        videoElement = localVideo;
    else
        videoElement = document.getElementById("remoteVideo_" + name);

    // Find the index of the videoElement in the array
    let index = videoElements.indexOf(videoElement);

    console.log("RemoveWebRTCTexture : " + "remoteVideo_" + name);
    if (index !== -1) {
        videoElements.splice(index, 1);

        console.log('WEBRTCTEXTURE_index:', index + '  videoElements:', videoElements.length);
        // Add the video element to the inactiveVideoElements list
        inactiveVideoElements.push(videoElement);
     }

}


function checkVideoElementReady(videoId, id) {
    function waitForElementAndCheckReadyState() {

        var video = document.getElementById(videoId);
        if (video) {
            // If the video element exists, check its ready state
            if (video.readyState >= 2) { // HAVE_CURRENT_DATA
               
                video.textureID = id;

                gameInstance.SendMessage('WebRTCVideo', 'OnVideoReady', videoId);
                
                
                 video.onloadedmetadata = function() {
                    
                    setTimeout(()=>
                    window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveDimensions', `${videoId},${video.videoWidth},${video.videoHeight}`)
                    , 100);
                    
            
                 };
          
          
          
            } else {
                // If not ready, check again after some time
                setTimeout(waitForElementAndCheckReadyState, 100);
            }
        } else {
            // If the video element does not exist yet, check again after some time
            console.log("Waiting for video element to be created:", videoId);
            setTimeout(waitForElementAndCheckReadyState, 100);
        }
    }

    waitForElementAndCheckReadyState();
}


// function SetupWebRTCTextureJS(id, name, GL, GLctx) {

//     let textureName = name;
//     let videoElement = document.getElementById("remoteVideo_" + textureName);
   
//     if (!videoElement)
//     videoElement = updateRemoteVideoElement(name);
   
//     // if (!videoElement) {
//     //     // If no existing video element is found, try to get an inactive video element from the pool
//     //     videoElement = inactiveVideoElements.pop();
//     // }

//     // if (!videoElement) {
//     //     console.log(`createdRemoteVideoElement : ` + "remoteVideo_" + textureName);
//     //     videoElement = document.createElement('video');
//     //     videoElement.id = "remoteVideo_" + textureName;//textureName;
//     //     videoElement.autoplay = true;
//     //     videoElement.playsInline = true;
//     //     videoElement.style.position = 'absolute';
//     //     videoElement.style.visibility = 'hidden';
//     //     videoElement.style.pointerEvents = 'none';

//     //     document.body.appendChild(videoElement); // Or append to a specific container

//     //     // videoElements.push(videoElement);


//     // }

//     videoElement.onloadedmetadata = function () {

//         window.gameInstance.SendMessage('WebRTCVideo', 'ReceiveDimensions', `${textureName},${videoElement.videoWidth},${videoElement.videoHeight}`);

//     };

//     // Create and fill a canvas with a gradient
//     ct_canvas = document.createElement("canvas");
//     ct_canvas.width = 256;
//     ct_canvas.height = 256;
//     ctx = ct_canvas.getContext("2d");
//     var grd = ctx.createLinearGradient(0, 0, 200, 0);
//     grd.addColorStop(0, "red");
//     grd.addColorStop(1, "white");
//     ctx.fillStyle = grd;
//     ctx.fillRect(0, 0, 256, 256);

//     // Get the WebGL texture object from the Emscripten texture ID.
//     textureObj = GL.textures[id];
//     // textureID = id;

//     // videoElement.textureID = id;

//     //if (!videoElements.includes(videoElement)) {
//      //   videoElements.push(videoElement);
//     //}
//     // if(videoElements.indexOf(videoElement) === -1) 
//     //   videoElements.push(videoElement);


//     console.log("WebGL texture created with ID:", id);

//     // GLctx is the webgl context of the Unity canvas
//     GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
//     GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
//     GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
//     GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);

//     // Upload the canvas image to the GPU texture.
//     GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);


//     function texturePaint(v) {

//         ctx.drawImage(v, 0, 0, 256, 256);

//         GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[v.textureID]);//textureObj);
//         GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
//         GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
//         GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);

//         GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, ct_canvas);

//     }

//     function updateTexture() {

//         for (let v of videoElements) {

//             if (v.readyState >= v.HAVE_CURRENT_DATA) {
//                 console.log("video ready" + v.id + "VIDEOELEMENTS :" + videoElements.length)
//                 texturePaint(v);
//             }

//         }
//         requestAnimationFrame(updateTexture);

//     }


//     //need to hijack this function for requestAnimationFrame to work during webxr session
//     xrManager.BrowserObject.requestAnimationFrame = function (func) {

//         if (xrManager.xrSession && xrManager.xrSession.isInSession) {
//             return xrManager.xrSession.requestAnimationFrame((time, xrFrame) => {
//                 xrManager.animate(xrFrame);

//                 for (let v of videoElements) {
//                     if (v.readyState >= v.HAVE_CURRENT_DATA)
//                         texturePaint(v);
//                 };
//                 // if (videoElement.readyState >= videoElement.HAVE_CURRENT_DATA) 
//                 //      texturePaint();

//                 func(time);

//             });

//         }
//         else {

//             window.requestAnimationFrame(func);

//         }
//     };

//     if (videoElements.length >= 0)
//         updateTexture();


// }
