getReference = function () {
    var controlSum = 0;

    function check() {
        controlSum = controlSum + 1;

        if (controlSum === 4) {
            return argument[0];
        }
    }

    function callback() {
        clearInterval(look);
        return 1;
    }

    function ajax(key) {
        var type = reference_arr.all[key];

        return $.ajax({
            url: (upload_via_pgu ?
                (cfgMainHost + '/common/ajax/index.php?ajaxModule=030301&ajaxAction=') : (reference_arr.url + '/')
            ) + type,
            type: 'GET',
            success: function (data) {                
                list_services_directory_json[type] = data;
                console.warn('Успешно загружен ' + type);
                check();
            },
            error: function () {
                console.error('Не удалось загрузить справочник ' + type);
                check();
            }
        });
    }

    var requests = [];

    for (var i in reference_arr.all) {
        if (reference_arr.all.hasOwnProperty(i)) {
            requests.push(ajax(i));
        }
    }

    $.when
        .apply(this, requests)
        .then(function () {
            start_form();
        },
            function () {
                $(document).ready(function () {
                    messagebox('', 'При обращении к сервису произошла ошибка.<br>Пожалуйста, попробуйте повторить запрос позже.');
                });
            }
        );
};