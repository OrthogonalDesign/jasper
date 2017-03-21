
export default function Communicator(dispatch, address, disconnect) {

  this.socket = new WebSocket(address)

  this.socket.onclose = function(){
    console.log('The socket closed');
    disconnect();
  };

  this.socket.onerror = (evt) => {
    console.log(JSON.stringify(evt));
  }

  this.socket.onmessage = (evt) => {
    const message = JSON.parse(evt.data)
    console.log('Got: ' + JSON.stringify(message) + ' with topic ' + message.type)

    // subject.next(message);
  }

  this.socket.onopen = () => {
    console.log('Opened a socket at ' + address)
    this.send({type: 'request-initial-data'})
  }

  this.send = (message) => {
    if (this.socket.readyState != 1) {
      disconnect();
    }
    else {
      const json = JSON.stringify(message)
      console.log('Sending to diagnostics: ' + json)
      this.socket.send(json)
    }
  }
}
