


var localStream;
var peerConnection;
var socket;



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

const constraints = {
    video: true,
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

        //   initializePeerConnection();


        // peerConnection.onicecandidate = event => {
        //     if (event.candidate) {
        //         socket.emit('candidate', event.candidate);
        //     }
        // };

        // peerConnection.oniceconnectionstatechange = () => {
        //     console.log('ICE connection state:', peerConnection.iceConnectionState);
        // };

    }).catch(error => console.error('Error accessing media devices:', error));
});


// function initializePeerConnection() {


//   //  setupSignaling();
// }

// function setupSignaling() {

//     socket.on('offer', (data) => {

//         if (!data.offer) {
//             console.error('Invalid offer received');
//             return;
//         }

//         peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer))
//             .then(() => peerConnection.createAnswer())
//             .then(answer => peerConnection.setLocalDescription(answer))
//             .then(() => socket.emit('answer', { answer: peerConnection.localDescription, target: data.sender }))
//             .catch(error => console.error('Error handling offer:', error));
//     });

//     socket.on('answer', data => {
//         if (data.answer) {
//             peerConnection.setRemoteDescription(new RTCSessionDescription(data.answer))
//                 .catch(error => console.error('Error handling answer:', error));
//         }
//     });

//     socket.on('candidate', data => {
//    if (data.candidate) {
//         peerConnection.addIceCandidate(new RTCIceCandidate(data.candidate))
//             .catch(error => console.error('Error adding ICE candidate:', error));

//     }

//     // peerConnection.addIceCandidate(new RTCIceCandidate(data.candidate))
//     //     .catch(error => console.error('Error adding ICE candidate:', error));
// });





// }

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

let availableClients = [];

let clientsOffered = []; // List of clients to whom an offer has been sent
let offers = []; // This should be your actual list of offers

const userName = "Rob-" + Math.floor(Math.random() * 100000)
const password = "x";

let offeredClients = new Set();
let didIOffer = false;

document.addEventListener('DOMContentLoaded', () => {
    connectAndSetupSocket();
});

function connectAndSetupSocket() {
    if (!socket) {
        socket = io('https://192.168.1.67:3000', {
            auth: {
                userName, password
            }
        });

        document.querySelector('#currentClientName').textContent = userName;
        // Set up socket event listeners
        setupSocketListeners();
    }
}

document.getElementById('callButton').addEventListener('click', () => {
    if (!socket) {
        socket = io('https://192.168.1.67:3000', {
            auth: {
                userName, password
            }
        });
 
        setupSocketListeners();
    }
    call();
});

function setupSocketListeners() {
    socket.on('updateOffers', updatedOffers => {
        updateOfferElements(updatedOffers);
    });

    socket.on('availableOffers', offers => {
        updateOfferElements(offers);
    });

    socket.on('newOfferAwaiting', offers => {
        updateOfferElements(offers);
    });

    socket.on('answerResponse', offerObj => {
        addAnswer(offerObj);
    });

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

        availableClients = clients;
        console.log(`Available clients: ${availableClients}`);
        
        updateClientElements(clients);
    });

    
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
                        newClientEl.innerHTML = `<button id="${client.userName}" style="position: relative; z-index: 1000;" class="btn btn-success col-1">Call ${client}</button>`;
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

// function updateClientElements(clients) {
//     const clientsEl = document.getElementById('clients'); // Adjust the ID as necessary
//     if (clientsEl) {
//         clientsEl.innerHTML = '';
//         clients.forEach(client => {
//             if (client !== userName) {
//                 const newClientEl = document.createElement('div');
//                 newClientEl.innerHTML = `<button id="${client}" class="btn btn-success col-1">Call ${client}</button>`;
//                 newClientEl.addEventListener('click', () => {
//                     call(client);
//                 });
//                 clientsEl.appendChild(newClientEl);
//             }
//         });
//     } else {
//         console.error('Element with ID "clients" not found');
//     }
// }


// function updateClientElements() {
//     const clientsEl = document.querySelector('#clients');
//     clientsEl.innerHTML = '';
//     availableClients.forEach(client => {
//         if (client !== userName) {
//         const newClientEl = document.createElement('div');
//         newClientEl.innerHTML = `<button id="${client}" class="btn btn-success col-1">Call ${client}</button>`;
//         newClientEl.addEventListener('click', () => {
//             call(client);
//         });
//         clientsEl.appendChild(newClientEl);
//     }
//     });
// }


function refreshAnswerButtons(updatedOffers) {
    // Assuming your answer buttons are in a container with an ID 'answer-container'
    const answerContainer = document.getElementById('answer-container');
    if (!answerContainer) return;

    // Clear existing buttons
    answerContainer.innerHTML = '';

    // Add new buttons based on updatedOffers
    updatedOffers.forEach(offer => {
        const button = document.createElement('button');
        button.className = 'btn btn-success';
        button.textContent = `Answer ${offer.offererUserName}`;
        button.onclick = () => answerOffer(offer); // Define this function to handle answer logic
        answerContainer.appendChild(button);
    });
}



function updateOfferElements(newOffers) {
    
    newOffers.forEach(newOffer => {
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





async function answerOffer(offerObj) {
    await fetchUserMedia();
    await createPeerConnection(offerObj);
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);
    offerObj.answer = answer;
    const offerIceCandidates = await socket.emitWithAck('newAnswer', offerObj);
    offerIceCandidates.forEach(c => {
        peerConnection.addIceCandidate(c);
    });
}

function addNewIceCandidate(iceCandidate) {
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

async function fetchUserMedia() {
    const stream = await navigator.mediaDevices.getUserMedia(constraints);
    localVideo.srcObject = stream;
    localStream = stream;
}

async function call(clientUserName) {

     // Check if an offer has already been sent to this client
    // if (clientsOffered.includes(clientUserName)) {
    //     console.log(`An offer has already been sent to ${clientUserName}`);
    //     return;
    // }
    
    await fetchUserMedia();
    await createPeerConnection();
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);
    didIOffer = true;
    socket.emit('newOffer', { offer, clientUserName });

 //   clientsOffered.push(clientUserName);
}

