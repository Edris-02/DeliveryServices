$(document).ready(function () {
    $('#addressInput').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: 'https://nominatim.openstreetmap.org/search',
                dataType: 'json',
                data: {
                    q: request.term,
                    format: 'json'
                },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.display_name,
                            value: item.display_name,
                            lat: item.lat,
                            lon: item.lon
                        };
                    }));
                }
            });
        },
        minLength: 2,
        select: function (event, ui) {
            console.log('Selected:', ui.item.label, 'Latitude:', ui.item.lat, 'Longitude:', ui.item.lon);
        }
    });

});
