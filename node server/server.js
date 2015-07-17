var fs = require('fs');

function readWriteAsync(string) {
  fs.readFile('Data.txt', 'utf-8', function(err, data){
    if (err) throw err;

    fs.writeFile('Data.txt', string, 'utf-8', function (err) {

      //console.log('filelistAsync complete');
    });
  });
}


var names = 0;

var positions = [];

var idstoname = [];


var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);

server.listen(80);

app.use(require('express').static(__dirname + ""))

app.get('/', function (req, res) {
  res.sendfile(__dirname);
});


io.sockets.on('connection', function (socket) {

  console.log("connected");
  socket.emit('name',{name: names});
  idstoname[names] = socket.id;
  names++;

  // wait for the event raised by the client
  socket.on('moved', function (data) {  
    //console.log(data.vector);
//    console.log("kevin #" + data.name + " moved to " + data.x + " " + data.y);

    positions[data.name] = {x: data.x, y: data.y};

    socket.emit('positions',positions);
    var stringed = positions.length+"\n";
    //console.log(stringed);
    for(var i = 0; i < positions.length; i++)
    {
      stringed = stringed + positions[i].x + "\n" + positions[i].y + "\n"
    }
    //console.log(stringed);
    readWriteAsync(stringed);
  });

  socket.on('disconnect', function () {
    console.log("rip gg " + socket.id);
    console.log("idstoname " + idstoname);
    for(var i = 0; i < idstoname.length; i++)
    {
      if(idstoname[i] == (socket.id))
      {
        console.log("asdasd");
        positions[i] = {x: 0, y: 0};
      }
    }
    });

});