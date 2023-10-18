// TO MAKE THE MAP APPEAR YOU MUST
// ADD YOUR ACCESS TOKEN FROM
// https://account.mapbox.com
var data = [];
var latitude = $(".latitude", this).text().trim();
var longitude = $(".longitude", this).text().trim();
var description = $(".description", this).text().trim();
var marker = {
    "latitude": latitude,
    "longitude": longitude,
    "descriptiom": description
};
data.push(marker);
mapboxgl.accessToken = 'pk.eyJ1IjoibGVvbmFyZG9wcmFzZXR5bzUiLCJhIjoiY2xsNTRvb2N4MGFtbzNlcGJ4cnk3ZTBqYSJ9.UK3uU3QgfCZ3hyc6pcVHJw';
const map = new mapboxgl.Map({
    container: 'map', // container ID
    // Choose from Mapbox's core styles, or make your own style with Mapbox Studio
    style: 'mapbox://styles/mapbox/streets-v12', // style URL
    center: [longitude, latitude], // starting position [lng, lat]
    zoom: 9 // starting zoom
});
map.on('load', function () {
    map.addLayer({
        "id": "places",
        "type": "symbol",
        "source": {
            "type": "geojson",
            "data": {
                "type": "FeatureCollection",
                "features": data
            }
        },
        "layout": {
            "icon-image": "{icon}",
            "icon-allow-overlap": true
        }
    })
})
