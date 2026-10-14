var datePicker = {
    controlFormat: 'dd-mm-yyyy',
    filterFormat: 'dd-MM-yyyy',
    hortDisplay: 'dd-MM-yyyy',
    rangeFormat: 'DD/MM/YYYY',
    getDate: function (a) {
        return new Date(a.getFullYear(), a.getMonth(), a.getDate);
    }
}


Date.isLeapYear = function (year) {
    return (((year % 4 === 0) && (year % 100 !== 0)) || (year % 400 === 0));
};

Date.getDaysInMonth = function (year, month) {
    return [31, (Date.isLeapYear(year) ? 29 : 28), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][month];
};

Date.prototype.isLeapYear = function () {
    return Date.isLeapYear(this.getFullYear());
};

Date.prototype.getDaysInMonth = function () {
    return Date.getDaysInMonth(this.getFullYear(), this.getMonth());
};

Date.prototype.addMonths = function (value) {
    var n = this.getDate();
    this.setDate(1);
    this.setMonth(this.getMonth() + value);
    this.setDate(Math.min(n, this.getDaysInMonth()));
    return this;
};

var app = angular.module('InfinityLab', ['ui.router', 'angucomplete-alt', 'ngRoute', 'LocalStorageModule',
    'angular-loading-bar', 'ngDialog', 'ngAnimate', 'toastr', 'cfp.hotkeys', 'chart.js', 'MassAutoComplete',
    'angularTreeview','angular-inview']);
//
//A Global Object which can be used throughout the application
//It contains the basic details like First Name, Last Name, Email etc..
app.value('UserDetails', {
    FirstName: '',
    LastName: '',
    Email: ''
});

app.value('version', '4');

app.factory('Table2Excel', function ($window, $rootScope) {
    var uri = 'data:application/vnd.ms-excel;base64,',
        template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',
        base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); },
        format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) };
    return {
        tableToExcel: function (tableContent, workdsheetName) {
            var ctx = { worksheet: workdsheetName, table: tableContent },
                href = uri + base64(format(template, ctx));
            return href;
        }
    };
})

app.factory('Excel', function ($window, $rootScope) {
    var uri = 'data:application/vnd.ms-excel;base64,',
        template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',
        base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); },
        format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) };
    return {
        tableToExcel: function (tableId, worksheetName, actioncolumncount, title) {
            //if (typeof (directhtml) == 'undefined') {

            if (!tableId.startsWith('#'))
                tableId = '#' + tableId;

            var table = $(tableId);
            var ctable = table.clone();
            var inputitems = ctable.find('INPUT');

            $.each(inputitems, function (k, item) {
                var $item = $(item);
                $item.replaceWith($item.val());
            });


            /*var headerrows = ctable.find('THEAD TR');
            var remorows = headerrows.length - 2;
            for (var index = 0; index < remorows; index++)
            {
                headerrows[index].remove();
            }*/

            ctable.find('THEAD TR.hidden-header').remove();

            if (typeof (ctable.attr('hcol')) != 'undefined') {
                var hiddencols = ctable.attr('hcol').split(',');
                for (var index = hiddencols.length - 1; index >= 0; index--) {
                    ctable.find('thead').find("tr").find("th:eq(" + index + ")").remove();
                    ctable.find('tbody').find("tr").find("td:eq(" + index + ")").remove();
                }
            }

            if (typeof (actioncolumncount) != 'undefined') {
                if (angular.isArray(actioncolumncount)) {
                    for (var index = actioncolumncount.length - 1; i >= 0; index--) {
                        ctable.find('thead').find("tr").find("th:eq(" + actioncolumncount[index] + ")").remove();
                        ctable.find('tbody').find("tr").find("td:eq(" + actioncolumncount[index] + ")").remove();
                    }
                }
                else {
                    for (var index = 0; index < actioncolumncount; index++) {
                        ctable.find("th:last-child").remove();
                        ctable.find("td:last-child").remove();
                    }
                }
            }

            var titlecolumns = ctable.find('THEAD TR TH').length;
            if (typeof (title) != 'undefined')
                ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + titlecolumns + "'>" + title + "</th></tr>");

            if ($rootScope.UserInfo.CompanyName != '')
                ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + titlecolumns + "'>" + $rootScope.UserInfo.CompanyName + "</th></tr>");

            var ctx = { worksheet: worksheetName, table: ctable[0].outerHTML },
                href = uri + base64(format(template, ctx));
            return href;
            //}
            //else {
            //    var ctx = { worksheet: worksheetName, table: directhtml },
            //        href = uri + base64(format(template, ctx));
            //}
        }
    };
})




app.filter('amountOnly', ['$filter', function ($filter) {
    return function (value) {
        if (value > 0)
            return $filter('number')(value, 2);
        return $filter('number')(value * -1, 2);
    }
}]);

app.filter('crOnly', ['$filter', function ($filter) {
    return function (value) {
        if (value > 0)
            return $filter('number')(value, 2) + ' Cr';
        return '';
    }
}]);

app.filter('bool2YesNo', ['$filter', function ($filter) {
    return function (value) {
        return value ? 'Yes' : 'No';
    }
}]);

app.filter('drOnly', ['$filter', function ($filter) {
    return function (value) {
        if (value < 0)
            return $filter('number')(value * -1, 2) + ' Dr';
        return '';
    }
}]);

app.filter('displayNumberOrEmpty', ['$filter', function ($filter) {
    return function (value) {
        if (value != 0)
            return $filter('number')(value, 0);
        return '';
    }
}]);

app.filter('displayEducation', ['$filter', function ($filter) {
    return function (value) {
        if (value)
            return 'Education';
        return 'Experience';
    }
}]);

app.filter('IndianNumber', ['$filter', function ($filter) {
    return function (value, decimalPoint) {

        if (!angular.isNumber(value))
            value = parseFloat(value);
        var svalue = value.toFixed(decimalPoint);
        var values = svalue.toString().split('.');

        var leftvalue = values[0].toString().replace(/(\d)(?=(\d\d)+\d$)/g, "$1,");
        if (values.length == 2)
            return leftvalue + "." + values[1];
        return leftvalue;

        //var newValue = [];

        //var intPart = values[0].split("").reverse();
        //var index = 0;
        //while (intPart.length > 0) {
        //    if ((index == 3 && intPart.length>3) || (index > 3 && index % 2 == 1))
        //        newValue.push(",");
        //    newValue.push(intPart.shift());
        //    index++;
        //}

        //newValue.reverse();
        //if (values.length == 0)
        //    return newValue.join("");
        //else
        //    return newValue.join("") + "." + values[1];
    }
}]);

app.filter('displayAsDate', ['$filter', function ($filter) {
    return function (date) {
        if (!angular.isDate(date)) {
            if (typeof (date) == 'string' && date.indexOf('T') > 0)
                return $filter('date')(new Date(date), datePicker.shortDisplay);
            return date;
        }

        return $filter('date')(date, datePicker.shortDisplay);
    }
}]);

app.filter('displayAsRupee', ['$filter', function ($filter) {
    return function (minute) {
        var fullvalue = parseFloat(minute);
        var rupee = Math.floor(fullvalue, 0);
        var paise = fullvalue - rupee;
        if (paise > 0 && rupee == 0)
            return 'Paise ' + Math.round(paise * 100, 0);
        return 'Rupee ' + minute;
    }
}]);

app.filter('displayMinuteAsHourMinute', ['$filter', function ($filter) {
    return function (minute) {
        var imin = parseInt(minute);

        if (minute == 0)
            return "";

        var remmin = imin % 60;
        var hour = (imin - remmin) / 60;

        return (hour < 10 ? '0' : '') + hour + (remmin < 10 ? ':0' : ':') + remmin;
    }
}]);

app.filter('displayMinuteAsTime', ['$filter', function ($filter) {
    return function (minute) {
        var imin = parseInt(minute);
        var remmin = imin % 60;
        var hour = (imin - remmin) / 60;
        if (hour < 12)
            return (hour < 10 ? '0' : '') + hour + (remmin < 10 ? ':0' : ':') + remmin + ' AM';
        else {
            hour -= 12;
            return (hour < 10 ? '0' : '') + hour + (remmin < 10 ? ':0' : ':') + remmin + ' PM';
        }
    }
}]);


app.directive('dateformat', ['$filter', function ($filter) {
    return {
        scope: true,
        restrict: 'A',
        require: '^ngModel',
        link: function (scope, element, attrs, ngModel) {
            var formatter = function (date) {
                if (typeof (date) == 'string' && date.indexOf('T') > 0)
                    return $filter('date')(new Date(date), datePicker.shortDisplay);

                return $filter('date')(date, datePicker.shortDisplay);
            };
            ngModel.$formatters.push(formatter);
        }
    }
}]);

app.directive('oneTimeCall', ['$parse', '$q', function ($parse, $q) {
    return {
        scope: true,
        restrict: 'A',
        link: function (scope, iEl, iAttrs) {
            var eventName;
            var $submitButton;
            var onClickOrSubmitFunction;

            if (iEl[0].tagName === 'FORM') {
                eventName = 'submit';
                $submitButton = angular.element(iEl[0].querySelector('[type=submit]'));
                onClickOrSubmitFunction = $parse(iAttrs.onSubmit);
            } else {
                eventName = 'click';
                $submitButton = iEl;
                onClickOrSubmitFunction = $parse(iAttrs.onClick);
            }

            // 0. the user clicks the button
            iEl.on(eventName, function () {
                scope.$apply(execute);
            });

            function execute() {
                var $body = angular.element(document.body);
                var buttonTextWhileDisabled = iAttrs.buttonTextWhileDisabled;
                var buttonTextWhileEnabled = $submitButton.text();
                var onClickOrSubmitReturnValue;

                var disableEverythingOnPage = !!iAttrs.disableEverythingOnPage;
                var $lightbox = angular.element(document.querySelector('.lightbox')); // http://stackoverflow.com/questions/1298034/disable-all-the-elements-in-html

                // 1. apply the disabled state
                applyDisabledState();

                // 2. run the function
                onClickOrSubmitReturnValue = onClickOrSubmitFunction(scope);
                onClickOrSubmitReturnValue = $q.when(onClickOrSubmitReturnValue);

                // 3. remove the disabled state when the functions promise resolves
                onClickOrSubmitReturnValue
                    .then(removeDisabledState)
                    .catch(removeDisabledState)
                    ;

                function applyDisabledState() {
                    $submitButton.prop('disabled', true);
                    $body.addClass('disable-double-submit');
                    $submitButton.addClass('disable-double-submit');

                    if (disableEverythingOnPage) {
                        $body.addClass('disable-everything-on-page');
                        $lightbox.addClass('lightbox-enabled');
                    }

                    if (buttonTextWhileDisabled) {
                        $submitButton.text(buttonTextWhileDisabled);
                    }
                }

                function removeDisabledState(response) {
                    $submitButton.prop('disabled', false);
                    $body.removeClass('disable-double-submit');
                    $submitButton.removeClass('disable-double-submit');

                    if (disableEverythingOnPage) {
                        $body.removeClass('disable-everything-on-page');
                        $lightbox.removeClass('lightbox-enabled');
                    }

                    if (buttonTextWhileDisabled) {
                        $submitButton.text(buttonTextWhileEnabled);
                    }

                    return response;
                }
            }
        }
    }
}]);

app.directive('flexibleWidth', function () {
    return function (scope, element, attr) {

        // Get parent elmenets width and subtract fixed width
        element.css({
            width: element.parent()[0].offsetWidth.toString() + 'px'
        });
    };
});

app.directive('backButton', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.on('click', function () {
                history.back();
                scope.$apply();
            });
        }
    };
});

app.directive('contextMenu', function ($parse) {
    var renderContextMenu = function ($scope, event, options) {
        if (!$) { var $ = angular.element; }
        $(event.currentTarget).addClass('context');
        var $contextMenu = $('<div>');
        $contextMenu.addClass('dropdown clearfix');
        var $ul = $('<ul>');
        $ul.addClass('dropdown-menu');
        $ul.attr({ 'role': 'menu' });
        $ul.css({
            display: 'block',
            position: 'absolute',
            left: event.pageX + 'px',
            top: event.pageY + 'px'
        });
        angular.forEach(options, function (item, i) {
            var $li = $('<li>');
            if (item === null) {
                $li.addClass('divider');
            } else {
                var $a = $('<a>');
                $a.attr({ tabindex: '-1', href: '#' });
                $a.text(typeof item[0] == 'string' ? item[0] : item[0].call($scope, $scope));
                $li.append($a);
                $li.on('click', function ($event) {
                    $event.preventDefault();
                    $scope.$apply(function () {
                        $(event.currentTarget).removeClass('context');
                        $contextMenu.remove();
                        item[1].call($scope, $scope);
                    });
                });
            }
            $ul.append($li);
        });
        $contextMenu.append($ul);
        var height = Math.max(
            document.body.scrollHeight, document.documentElement.scrollHeight,
            document.body.offsetHeight, document.documentElement.offsetHeight,
            document.body.clientHeight, document.documentElement.clientHeight
        );
        $contextMenu.css({
            width: '100%',
            height: height + 'px',
            position: 'absolute',
            top: 0,
            left: 0,
            zIndex: 9999
        });
        $(document).find('body').append($contextMenu);
        $contextMenu.on("mousedown", function (e) {
            if ($(e.target).hasClass('dropdown')) {
                $(event.currentTarget).removeClass('context');
                $contextMenu.remove();
            }
        }).on('contextmenu', function (event) {
            $(event.currentTarget).removeClass('context');
            event.preventDefault();
            $contextMenu.remove();
        });
    };
    return function ($scope, element, attrs) {
        element.on('contextmenu', function (event) {
            $scope.$apply(function () {
                event.preventDefault();
                var options = $scope.$eval(attrs.contextMenu);
                if (options instanceof Array) {
                    renderContextMenu($scope, event, options);
                } else {
                    throw '"' + attrs.contextMenu + '" not an array';
                }
            });
        });
    };
});

app.directive('singleclick', function () {
    return {
        restrict: 'EA',
        link: function (scope, iElement, iAttrs) {
            iElement.bind('click', function () {
                iElement.prop('disabled', true);
                scope.singleclick().finally(function () {
                    iElement.prop('disabled', false);
                })
            });
        }
    };
});

app.directive('daterange', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {

            

            var drops = 'auto';
            var opens = 'right';

            if (typeof (attrs['drops']) != 'undefined')
                drops = attrs['drops'];

            if (typeof (attrs['opens']) != 'undefined')
                opens = attrs['opens'];

            var $el = $(element);
            var dp = $el.daterangepicker({
                autoclose: true,
                adaptivePosition: true,
                autoApply: true,     
                autoUpdateInput: true,
                adaptivePosition:true,
                format: datePicker.rangeFormat,
                opens: opens,
                drops: drops
            });

            const isMobile = window && window.matchMedia && window.matchMedia('(pointer:coarse)').matches;
            if (isMobile) {

                // 2) On mobile: prevent input from auto-opening & from showing keyboard
                $el.off('focus.daterangepicker click.daterangepicker').attr('readonly', true);

                // 3) Inject a button (only once)
                // Wrap the input so we can place the button neatly
                if (!$el.parent().hasClass('drp-inline-wrap')) {
                    $el.wrap(
                        "<div class='drp-inline-wrap' " +
                        "style='display:flex;align-items:center;gap:6px;position:relative;'></div>"
                    );
                }

                if ($el.next('.drp-trigger').length === 0) {
                    const btnHtml =
                        "<button type='button' class='drp-trigger' " +
                        "aria-label='Open calendar' " +
                        "style='display:inline-flex;align-items:center;justify-content:center;" +
                        "height:34px;min-width:34px;padding:0 8px;border:1px solid #ccc;border-radius:4px;" +
                        "background:#fff;cursor:pointer;'>\uD83D\uDCC5</button>";

                    const $btn = $(btnHtml).insertAfter($el);

                    // 4) Open the picker from the button
                    $btn.on('click', function () {
                        const drp = $el.data('daterangepicker');
                        if (drp) drp.show();
                    });
                }
            }

            $(element).change(function () {
                if (this.value == '') {
                    $parse(dateModelName[0]).assign(scope, '');
                    $parse(dateModelName[1]).assign(scope, '');
                }
            });

            var dpg = $.fn.datepicker.DPGlobal;
            var dpgformat = dpg.parseFormat(datePicker.rangeFormat);

            var dateModelName = attrs['daterange'].split(',');
            if (dateModelName.length != 2) return;

            dp.on('apply.daterangepicker', function (ev, picker) {
                $parse(dateModelName[0]).assign(scope, picker.startDate.format('LL'));
                $parse(dateModelName[1]).assign(scope, picker.endDate.format('LL'));
            });

            scope.$watch(dateModelName[0], function (newval, oldVal) {
                if (typeof (newval) != 'undefined' && newval != null && newval != '') {
                    if (typeof (newval) == 'string' && newval.length != 10) {
                        newval = new Date(newval);
                        dp.data('daterangepicker').setStartDate(newval);
                    }
                }
                else {
                    //dp.data('daterangepicker').setStartDate('');
                }
            });

            scope.$watch(dateModelName[1], function (newval, oldVal) {
                if (typeof (newval) != 'undefined' && newval != null && newval != '') {
                    if (typeof (newval) == 'string' && newval.length != 10) {
                        newval = new Date(newval);
                        dp.data('daterangepicker').setEndDate(newval);
                    }
                } else {
                    //dp.data('daterangepicker').setEndDate('');
                }
            });

        }
    }
}]);


app.directive("datemodule", ['$parse', '$filter', '$compile', function ($parse, $filter, $compile) {
    return {
        restrict: "A",
        require: '^ngModel',
        link: function (scope, element, attrs, ngModel) {

            var dateModelName = attrs['datemodule'];
            var parts = dateModelName.split('.');
            var ngmodelnew = '';
            for (var i = 0; i < parts.length; i++)
                ngmodelnew += (ngmodelnew.length > 0 ? '.' : '') + (parts.length == i + 1 ? '_' : '') + parts[i];

            if (attrs['ngModel'] != ngmodelnew) {
                element.removeAttr('ng-model');
                element.attr('ng-model', ngmodelnew);
                element.unbind();
                $compile(element)(scope);
                return;
            }
            var $el = $(element);
            var dp = $el.datepicker({
                autoclose: true,
                format: datePicker.controlFormat,
                constrainInput: false,
                immediateUpdates: true,
                todayBtn: true,
                todayHighlight: true
            });

            const isMobile = window && window.matchMedia && window.matchMedia('(pointer:coarse)').matches;
            if (isMobile) {
                // 1) Prevent input from auto-opening & from showing keyboard
                $el.off('focus.datepicker click.datepicker').attr('readonly', true);

                // 2) Wrap the input so we can place the button neatly
                if (!$el.parent().hasClass('dp-inline-wrap')) {
                    $el.wrap(
                        "<div class='dp-inline-wrap' " +
                        "style='display:flex;align-items:center;gap:6px;position:relative;'></div>"
                    );
                }

                // 3) Inject a button (only once)
                if ($el.next('.dp-trigger').length === 0) {
                    const btnHtml =
                        "<button type='button' class='drp-trigger' " +
                        "aria-label='Open calendar' " +
                        "style='display:inline-flex;align-items:center;justify-content:center;" +
                        "height:34px;min-width:34px;padding:0 8px;border:1px solid #ccc;border-radius:4px;" +
                        "background:#fff;cursor:pointer;'>\uD83D\uDCC5</button>";

                    const $btn = $(btnHtml).insertAfter($el);

                    // 4) Open the datepicker from the button
                    $btn.on('click', function () {
                        if ($el.datepicker) { // jQuery UI Datepicker
                            $el.datepicker('show');
                        } else if ($el.data('datepicker')) { // Bootstrap Datepicker
                            $el.datepicker('show');
                        }
                    });
                }
            }

            dp.on('changeDate', function (e) {
                var inputvalue = $(e.target).val();

                if (typeof (inputvalue) != 'undefined' && inputvalue != null && inputvalue.length >= datePicker.controlFormat.length) {
                    var newval = e.date;
                    newval = new Date(Date.UTC(newval.getFullYear(), newval.getMonth(), newval.getDate()));
                    //var TimeZoned = new Date(e.date.setTime(e.date.getTime() + (e.date.getTimezoneOffset() * 60000)));
                    $parse(dateModelName).assign(scope, newval);
                    console.info('changeDate' + dpg.formatDate(newval, datePicker.controlFormat, "en"))
                }
                //$parse(ngmodelnew).assign(scope, dpg.formatDate(newval, datePicker.controlFormat, "en"));

            });

            var dpg = $.fn.datepicker.DPGlobal;
            var dpgformat = dpg.parseFormat(datePicker.controlFormat);

            //ngModel.$viewChangeListeners.push(function () {
            //    if (ngModel.$viewValue != '') {
            //        var date = dpg.parseDate(ngModel.$viewValue, dpgformat);
            //        var currentData = scope.$eval(dateModelName);
            //        if (typeof (currentData) == 'string' && currentData.length != 10) {
            //            currentData = new Date(currentData);
            //        }
            //        if (angular.isDate(date) && date.getFullYear() == 1) {
            //            dp.datepicker('update', '');
            //        } else if (currentData != date) {
            //            $parse(dateModelName).assign(scope, date);
            //            //this.formatSsn($ngModel.$modelValue)
            //            //ngModel.$setViewValue(dpg.formatDate(date, datePicker.controlFormat, "en"));
            //            //ngModel.$render();
            //        }
            //    } else {
            //        $parse(dateModelName).assign(scope, '');
            //    }
            //});

            scope.$watch(dateModelName, function (newval, oldVal) {
                if (typeof (newval) != 'undefined' && newval != null && newval != '') {
                    if (typeof (newval) == 'string' && newval.length != 10) {
                        newval = new Date(newval);
                        newval = new Date(Date.UTC(newval.getFullYear(), newval.getMonth(), newval.getDate()))
                    }

                    if (angular.isDate(newval) && newval.getFullYear() != 1) {
                        if (ngModel.$viewValue != dpg.formatDate(newval, datePicker.controlFormat, "en")) {
                            //var currentdpdate = dp.datepicker('getUTCDate');
                            //if (dp.datepicker('getDate') != new Date(newval)) {
                            console.info('setUTCDate' + dpg.formatDate(newval, datePicker.controlFormat, "en"));

                            dp.datepicker("setDate", newval);
                            dp.datepicker('update');

                            //dp.datepicker('update', dpg.formatDate(newval, datePicker.controlFormat, "en"));
                            //dp.datepicker('update', newval);
                            //    //alert('watch-' + dpg.formatDate(newval, datePicker.controlFormat, "en"))

                            //    //this.formatSsn($ngModel.$modelValue)
                            //    //ngModel.$setViewValue(dpg.formatDate(newval, datePicker.controlFormat, "en"));
                            //    //ngModel.$render();
                            //}
                        }
                    }
                }
                if (newval) {
                    if (dp.val() != dpg.formatDate(newval, datePicker.controlFormat, "en")) {
                        dp.val(dpg.formatDate(newval, datePicker.controlFormat, "en"));
                    }
                }
            });
        }
    };
}]);


/*app.filter('displayAsDate', ['$filter', function ($filter) {
    return function (date) {
        if (!angular.isDate(date))
            return date;

        return $filter('date')(date, datePicker.shortDisplay);
    }
}]);


app.directive('dateformat', ['$filter', function($filter) {
    return {
        scope: true,
        restrict: 'A',
        require: '^ngModel',
        link: function (scope, element, attrs, ngModel) {
            var formatter = function (value) {
                // the currency filter returns undefined if parse error
                return $filter('date')(date, datePicker.shortDisplay);
            };
            ngModel.$formatters.push(formatter);
        }
    }
}]);*/

app.directive('selectAllOnFocus', function () {
    'use strict';

    return {
        restrict: 'A',
        scope: {
            selectAllOnFocus: '=',
        },
        link: function (scope, element, attrs, ctrl) {
            element.on('focus', function () {
                var domElement = element[0];
                if (scope.selectAllOnFocus &&
                    angular.isFunction(domElement.setSelectionRange)) {
                    domElement.setSelectionRange(0, domElement.value.length);
                }
            });
        },
    };
});

app.directive('infiniteScrollDIV', [
    '$rootScope', '$window', '$timeout', function ($rootScope, $window, $timeout) {
        return {
            link: function (scope, elem, attrs) {
                var checkWhenEnabled, handler, scrollDistance, scrollEnabled;
                $window = angular.element($window);
                elem.css('overflow-y', 'scroll');
                elem.css('overflow-x', 'hidden');
                elem.css('height', 'inherit');
                scrollDistance = 0;
                if (attrs.infinitescrollDistance != null) {
                    scope.$watch(attrs.infinitescrollDistance, function (value) {
                        return scrollDistance = parseInt(value, 10);
                    });
                }
                scrollEnabled = true;
                checkWhenEnabled = false;
                if (attrs.infinitescrollDisabled != null) {
                    scope.$watch(attrs.infinitescrollDisabled, function (value) {
                        scrollEnabled = !value;
                        if (scrollEnabled && checkWhenEnabled) {
                            checkWhenEnabled = false;
                            return handler();
                        }
                    });
                }
                $rootScope.$on('refreshStart', function (event, parameters) {
                    elem.animate({ scrollTop: "0" });
                });
                handler = function () {
                    var container, elementBottom, remaining, shouldScroll, containerBottom;
                    container = $(elem.children()[0]);
                    elementBottom = elem.offset().top + elem.height();
                    containerBottom = container.offset().top + container.height();
                    remaining = containerBottom - elementBottom;
                    shouldScroll = remaining <= elem.height() * scrollDistance;
                    if (shouldScroll && scrollEnabled) {
                        if ($rootScope.$$phase) {
                            return scope.$eval(attrs.infinitescroll);
                        } else {
                            return scope.$apply(attrs.infinitescroll);
                        }
                    } else if (shouldScroll) {
                        return checkWhenEnabled = true;
                    }
                };
                elem.on('scroll', handler);
                scope.$on('$destroy', function () {
                    return $window.off('scroll', handler);
                });
                return $timeout((function () {
                    if (attrs.infinitescrollImmediateCheck) {
                        if (scope.$eval(attrs.infinitescrollImmediateCheck)) {
                            return handler();
                        }
                    } else {
                        return handler();
                    }
                }), 0);
            }
        };
    }
]);

app.directive('fixedHeader', ['$compile', '$window', '$rootScope', '$timeout', function ($compile, $window, $rootScope, $timeout) {
    return function link(scope, elem, attrs) {
        // Wrap the contents of every header cell with div.th-inner so that the
        // CSS can relocate it.
        var height = attrs['fixedHeader'];
        var additionalheight = attrs['additionalheight'];
        if (typeof (additionalheight) != 'undefined') {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            var iHeight = available - (100 + parseInt(additionalheight));
            if (typeof (iHeight) != 'undefined' && !isNaN(iHeight) && iHeight > 100) {
                height = iHeight + 'px';
            }
        }


        var header = elem.find('thead').find('tr');
        angular.forEach(header.find('th'), function (th) {
            var newelement = angular.element(th).contents().wrap('<div class="th-inner"></div>');
        });

        var actualHeight = 0;

        $timeout(function () {
            // wrap the table in a couple of divs that bring in important css modifications
            /*if (typeof ($rootScope.windowSize) != 'undefined') {
                actualHeight = parseInt(height.replace('px', ''));
                var rheight = $rootScope.windowSize.height - ($(elem)[0].offsetTop+110);
                if (rheight < actualHeight)
                    height = '' + (rheight) + 'px';
            }*/

            var outerdiv = elem.wrap('<div class="fixed-table-container" style="height:' + height + '"></div>').parent();
            var innerdiv = elem.wrap('<div class="fixed-table-container-inner"></div>').parent();

            //$rootScope.$on('windowSizeChanged', function(newsize) {
            //    var rheight = $rootScope.windowSize.height - ($(elem)[0].offsetTop + 110);
            //    if (rheight < actualHeight)
            //        height = '' + (rheight) + 'px';
            //    else
            //        height = '' + actualHeight + 'px';
            //    $(outerdiv).css('height', height);
            //});

            var scrollfunction = function () {
                if (innerdiv.scrollTop() + innerdiv.height() == innerdiv[0].scrollHeight) {
                    scope.$apply(function () {
                        scope.$eval(attrs.infinitescroll);
                    });
                }
            };

            if (typeof (attrs.infinitescroll) != 'undefined') {
                innerdiv.on('scroll', scrollfunction);
                outerdiv.on('scroll', scrollfunction);
            }

            elem[0].resetcolumns = function () {
                elem.find('thead').find("tr").find("th").show();
                elem.find('tbody').find("tr").find("td").show();
            }

            if (typeof (attrs['clickonremove']) != 'undefined' &&
                attrs['clickonremove'] == 'true') {

                elem.find('.th-inner').bind('dblclick', function (e) {
                    var colnum = $(this).parent().closest("th").prevAll("th").length;
                    var table = $(this).parent().closest("table");
                    if (typeof (table.attr('hcol')) != 'undefined')
                        table.attr('hcol', table.attr('hcol') + "," + colnum);
                    else
                        table.attr('hcol', colnum);
                    //$(this).closest("table").find("tr").find("td:eq(" + colnum + ")").remove();
                    table.find('thead').find("tr").find("th:eq(" + colnum + ")").hide();
                    table.find('tbody').find("tr").find("td:eq(" + colnum + ")").hide();
                });
            }
        }, 50);

    };
}]);


app.directive('autoSize', ['$compile', '$window', '$rootScope', '$timeout', function ($compile, $window, $rootScope, $timeout) {
    return function link(scope, elem, attrs) {
        angular.element(window).resize(function () {
            scope.winHeight();
        });
        setTimeout(function () {
            scope.winHeight();
        }, 150);
        scope.winHeight = function () {
            element.css('height', (angular.element(window).height() - 300) + 'px');
        }
    };
}]);


app.directive('fixHead', ['$compile', '$filter', function ($compile, $window) {

    function compile(tElement) {
        var table = {
            clone: tElement.parent().clone().empty(),
            original: tElement.parent()
        };

        var header = {
            clone: tElement.clone(),
            original: tElement
        };

        // prevent recursive compilation
        header.clone.removeAttr('fix-head').removeAttr('ng-if');

        table.clone.css({ display: 'block', overflow: 'hidden' }).addClass('clone');
        header.clone.css('display', 'block');
        header.original.css('display', 'none');

        return function postLink(scope) {
            var scrollContainer = table.original.parent();

            // insert the element so when it is compiled it will link
            // with the correct scope and controllers
            header.original.after(header.clone);

            $compile(table.clone)(scope);
            $compile(header.clone)(scope);

            scrollContainer.parent()[0].insertBefore(table.clone.append(header.clone)[0], scrollContainer[0]);

            scrollContainer.on('scroll', function () {
                // use CSS transforms to move the cloned header when the table is scrolled horizontally
                header.clone.css('transform', 'translate3d(' + -(scrollContainer.prop('scrollLeft')) + 'px, 0, 0)');
            });

            function cells() {
                return header.clone.find('th').length;
            }

            function getCells(node) {
                return Array.prototype.map.call(node.find('th'), function (cell) {
                    return jQLite(cell);
                });
            }

            function height() {
                return header.original.prop('clientHeight');
            }

            function jQLite(node) {
                return angular.element(node);
            }

            function marginTop(height) {
                table.original.css('marginTop', '-' + height + 'px');
            }

            function updateCells() {
                var cells = {
                    clone: getCells(header.clone),
                    original: getCells(header.original)
                };

                cells.clone.forEach(function (clone, index) {
                    if (clone.data('isClone')) {
                        return;
                    }

                    // prevent duplicating watch listeners
                    clone.data('isClone', true);

                    var cell = cells.original[index];
                    var style = $window.getComputedStyle(cell[0]);

                    var getWidth = function () {
                        return style.width;
                    };

                    var setWidth = function () {
                        marginTop(height());
                        clone.css({ minWidth: style.width, maxWidth: style.width });
                    };

                    var listener = scope.$watch(getWidth, setWidth);

                    $window.addEventListener('resize', setWidth);

                    clone.on('$destroy', function () {
                        listener();
                        $window.removeEventListener('resize', setWidth);
                    });

                    cell.on('$destroy', function () {
                        clone.remove();
                    });
                });
            }

            scope.$watch(cells, updateCells);

            header.original.on('$destroy', function () {
                header.clone.remove();
            });
        };
    }

    return {
        compile: compile
    };

}]);





app.directive('selfvalidator', ['$parse', '$filter', '$timeout', '$rootScope', function ($parse, $filter, $timeout, $rootScope) {
    return {
        restrict: 'EA',
        require: 'form',
        scope: true,
        link: function (scope, element, attrs, formCtrl) {
            if (typeof (attrs.selfvalidator) == 'undefined' || attrs.selfvalidator == null)
                attrs.selfvalidator = '';
            var actualMethod = scope[attrs.selfvalidator];
            var errorList = [];
            scope[attrs.selfvalidator] = function () {

                angular.forEach(errorList, function (element) {
                    element.removeClass('border-error');
                });
                var errors = [];
                if (formCtrl.$invalid) {
                    var list = element.find('.ng-invalid');
                    angular.forEach(list, function (el) {
                        var elem = angular.element(el);
                        errors.push(elem);
                        elem.addClass('border-error');

                        elem.on('blur', function () {
                            var elem = angular.element(this);
                            if (elem.hasClass('ng-valid'))
                                elem.removeClass('border-error');
                        });
                    });
                    if (list.length > 1)
                        list.first().focus();
                }

                if (errors.length == 0) {
                    scope.$eval(actualMethod);
                }
                else {
                    errorList = errors;
                    $rootScope.Toaster.Error("Contains (" + errors.length + ") Input Errors");
                }
            };
        }
    }
}]);

/*
app.directive('select2', ['$parse', '$filter', '$timeout', function ($parse, $filter, $timeout) {
    return {
        restrict: 'A',
        require: '^ngModel',
        link: function (scope, element, attrs, ngModel) {
            if (typeof (attrs.select2) == 'undefined' || attrs.select2 == null)
                attrs.select2 = '';
            var viewModel = attrs['viewmodel'];
            var el = $(element);
            var sel2 = el.select2({
                allowClear: true,
                placeholder: attrs.select2,
                multiple: false,
                maximumSelectionSize: 1
            });

            
                scope.$watch(attrs['ngModel'], function (newvalue, oldvalue) {
                    if (el.val() != newvalue)
                        el.val(newvalue).trigger('change.select2');
                });
            
            if (typeof (viewModel) != 'undefined' && viewModel.length > 0) {
                sel2.on("change", function (e) {
                    var selectedValue = el.select2('data');
                    if (selectedValue && typeof (selectedValue) != 'undefined'
                        && angular.isArray(selectedValue) && selectedValue.length == 1) {
                        $parse(viewModel).assign(scope, selectedValue[0].text);
                    }
                });
            }
        }
    }
}]);*/


app.directive('select2', ['$parse', '$filter', '$timeout', '$rootScope', function ($parse, $filter, $timeout, $rootScope) {
    return {
        restrict: 'A',
        require: '^ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (typeof (attrs.select2) == 'undefined' || attrs.select2 == null)
                attrs.select2 = '';

            var isactive = true;
            var viewModel = attrs['viewmodel'];
            var el = $(element);
            var sel2 = null;
            var startswith = function matchStart(params, data) {
                params.term = params.term || '';
                if (data.text.toUpperCase().indexOf(params.term.toUpperCase()) == 0) {
                    return data;
                }
                return false;
            };

            if (typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null && $rootScope.UserInfo.appsettings.system_ctrl_select2_startsWith) {
                if (typeof (attrs.startswith) == 'undefined')
                    attrs.startswith = 'true';
            }

            var multiple = false;
            if (typeof (attrs.multiple) != 'undefined' && attrs.multiple.toLowerCase() == 'true')
                multiple = true;

            var databased = false;
            var hasstartwith = typeof (attrs.startswith) != 'undefined' && attrs.startswith == 'true';
            var enableadd = typeof (attrs.enableadd) != 'undefined' && attrs.enableadd == 'true';
            var s2rendered = false;
            var s2currentvalue = null;
            if (typeof (attrs['options']) != 'undefined') {
                //"city.id as city.name for city in Cities track by city.id"
                var options = attrs['options'].split(' ');
                if (options.length != 10) {
                    options = attrs['options'].replace('\n', '').replace('\r', '').split(' ').filter(function (item) {
                        return item != null && item != '';
                    });
                }
                var idfield = options[0].split('.')[1];
                var displayfield = options[2].split('.')[1];
                var datasource = options[6];
                scope.$watch(datasource, function (newvalue, oldvalue) {
                    if (newvalue && newvalue != null && typeof (newvalue.length) != 'undefined') {
                        if (s2rendered)
                            el.select2("destroy");

                        if (typeof (newvalue) == 'undefined' || newvalue == null)
                            newvalue = [];

                        var data = $.map(newvalue, function (obj) {
                            obj.id = obj[idfield];
                            obj.text = obj[displayfield];
                            if (Number.isInteger(obj.id) && obj.id == 0) obj.id = '0';
                            return obj;
                        });

                        var initData = null;
                        var modelValue = $parse(attrs.ngModel)(scope);
                        if (modelValue && modelValue != null) {
                            initData = data.filter(function (item) {
                                return item.id == modelValue
                            });
                        }
                        sel2 = el.select2({
                            allowClear: true,
                            placeholder: attrs.select2,
                            maximumSelectionSize: 1,
                            multiple: multiple,
                            matcher: startswith,
                            idfield: idfield,
                            _data: newvalue,
                            hasstartwith: hasstartwith,
                            ajax: {},
                            tags: enableadd,
                            initData: initData,
                            dataAdapter: Select2Adaptor,
                            displayfield: displayfield
                        });
                        s2rendered = true;
                        //el.datasource = newvalue;
                        el.eventcount = 0;

                        scope.$watch(attrs['ngModel'], function (newvalue, oldvalue) {
                            if (s2currentvalue != newvalue) {
                                var control = el.data('select2');
                                var adapter = control.dataAdapter;
                                if (adapter.setSelected) {
                                    control.trigger('selection:update', {
                                        data: [adapter.setSelected(newvalue)]
                                    });
                                    control.trigger('change.select2');
                                    s2currentvalue = newvalue;
                                }
                                else {
                                    $timeout(function () {
                                        if (adapter.setSelected) {
                                            control.trigger('selection:update', {
                                                data: [adapter.setSelected(newvalue)]
                                            });
                                            control.trigger('change.select2');
                                            s2currentvalue = newvalue;
                                        }
                                    }, 50)
                                }
                            }
                        });
                        if (typeof (attrs.focusonenter) != 'undefined' || typeof (attrs.onkeyenter) != 'undefined') {
                            sel2.on('select2:select', function (event) {
                                var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                                scope.$apply(function () {
                                    isactive = false;
                                    scope.$eval(fooName);
                                });
                            });
                            sel2.on('select2:close', function (event) {
                                if (event.params && event.params.enterkey) {
                                    var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                                    scope.$apply(function () {
                                        isactive = false;
                                        scope.$eval(fooName);
                                    });
                                }
                            });
                        }

                    }
                });

                if (typeof (scope[datasource]) != 'undefined' && scope[datasource] != null) {
                    if (typeof (scope[datasource].length) != 'undefined') {
                        var data = $.map(scope[datasource], function (obj) {
                            obj.id = obj[idfield];
                            obj.text = obj[displayfield];
                            if (Number.isInteger(obj.id) && obj.id == 0) obj.id = '0';
                            return obj;
                        });
                        var initData = null;
                        var modelValue = $parse(attrs.ngModel)(scope);
                        if (modelValue && modelValue != null) {
                            initData = data.filter(function (item) {
                                return item.id == modelValue
                            });
                        }
                        sel2 = el.select2({
                            allowClear: true,
                            multiple: multiple,
                            initData: initData,
                            placeholder: attrs.select2,
                            maximumSelectionSize: 1,
                            matcher: startswith,
                            hasstartwith: hasstartwith,
                            idfield: idfield,
                            _data: scope[datasource],
                            ajax: {},
                            tags: enableadd,
                            dataAdapter: Select2Adaptor,
                            displayfield: displayfield
                        });

                        var modelValue = $parse(attrs.ngModel)(scope);
                        if (typeof (modelValue) != 'undefined') {
                            if (el.val() != modelValue) {
                                $timeout(function () {
                                    var control = el.data('select2');
                                    var adapter = control.dataAdapter;
                                    control.trigger('selection:update', {
                                        data: adapter.setSelected ? [adapter.setSelected(modelValue)] : []
                                    });
                                    control.trigger('change.select2');
                                }, 10);
                            }
                        }

                        s2rendered = true;
                        if (typeof (attrs.focusonenter) != 'undefined' || typeof (attrs.onkeyenter) != 'undefined') {
                            sel2.on('select2:select', function (event) {
                                var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                                scope.$apply(function () {
                                    isactive = false;
                                    scope.$eval(fooName);
                                });
                            });
                            sel2.on('select2:close', function (event) {
                                if (event.params && event.params.enterkey) {
                                    var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                                    scope.$apply(function () {
                                        isactive = false;
                                        scope.$eval(fooName);
                                    });
                                }
                            });
                        }
                        if (modelValue && modelValue != null) {
                            var control = el.data('select2');
                            var adapter = control.dataAdapter;

                            if (adapter.setSelected == undefined) {
                                adapter.setSelected = function (value) {
                                    var s = this.options.options._data.find(function (e) {
                                        return e.id == value;
                                    });
                                    if (s) return s;
                                    return { 'id': 0 };
                                }
                            }

                            control.trigger('selection:update', {
                                data: [adapter.setSelected(modelValue)]
                            });
                            control.trigger('change.select2');
                        }
                    }
                }
                databased = true;
            }

            if (!databased) {
                if (typeof (attrs.startswith) != 'undefined' && attrs.startswith == 'true') {
                    sel2 = el.select2({
                        allowClear: true,
                        placeholder: attrs.select2,
                        multiple: multiple,
                        maximumSelectionSize: 1,
                        matcher: startswith,
                        tags: enableadd
                    });
                }
                else {
                    sel2 = el.select2({
                        allowClear: true,
                        placeholder: attrs.select2,
                        multiple: multiple,
                        maximumSelectionSize: 1,
                        tags: enableadd
                    });
                }

                scope.$watch(attrs['ngModel'], function (newvalue, oldvalue) {
                    if (el.val() != newvalue) {
                        el.val(newvalue).trigger('change.select2');
                    }
                });


                var modelValue = $parse(attrs.ngModel)(scope);
                if (typeof (modelValue) != 'undefined') {
                    if (el.val() != modelValue) {
                        $timeout(function () {
                            s2currentvalue = modelValue;
                            el.val(modelValue).trigger('change.select2');
                        }, 10);
                    }
                }


                if (typeof (attrs.focusonenter) != 'undefined' || typeof (attrs.onkeyenter) != 'undefined') {
                    sel2.on('select2:select', function (event) {
                        var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                        scope.$apply(function () {
                            isactive = false;
                            scope.$eval(fooName);
                        });
                    });
                    sel2.on('select2:close', function (event) {
                        if (event.params && event.params.enterkey) {
                            var fooName = typeof (attrs.focusonenter) != 'undefined' ? ('setFocus' + attrs.focusonenter + "()") : attrs.onkeyenter;
                            scope.$apply(function () {
                                isactive = false;
                                scope.$eval(fooName);
                            });
                        }
                    });
                }
            }

            var select2Focus = function () {
                $(this).closest('.select2').prev('select').select2('open');
            };

            el.next('.select2').find('.select2-selection').one('focus', select2Focus).on('blur', function () {
                $(this).one('focus', select2Focus)
            })

            //if (enableadd && sel2 && sel2!=null) {
            //    var currentQuery = '';
            //    sel2.on('select2:closing', function (e) {
            //        currentQuery = $('.select2-search input').prop('value');
            //    });

            //    sel2.on('select2:close', function (e) {
            //        //scope.$eval(attrs['ngModel'])
            //        //$('.select2-search input').val(currentQuery).trigger('change');
            //    });
            //}

            if (typeof (attrs.focusid) != 'undefined') {
                var fooName = 'setFocus' + attrs.focusid;
                scope[fooName] = function () {
                    el[0].focus();
                    if (el.data('select2') != null)
                        el.select2('open');
                }
            }

            //if (typeof (viewModel) != 'undefined' && viewModel.length > 0) {
            //    if (sel2) {
            //        sel2.on("change", function (e) {
            //            var selectedValue = el.select2('data');
            //            if (selectedValue && typeof (selectedValue) != 'undefined'
            //                && angular.isArray(selectedValue) && selectedValue.length == 1) {
            //                $parse(viewModel).assign(scope, selectedValue[0].text);
            //            }
            //        });
            //    }
            //}
        }
    }
}]);


app.directive('number', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, modelCtrl) {

            var regexp = /[^0-9]/g;

            if (typeof (attrs.number) == 'undefined' || attrs.number == null || attrs.number == '')
                attrs.format = 'integer';
            switch (attrs.format) {
                case 'integer':
                    regexp = /[^0-9]/g;
                    break;
                case 'decimal':
                    regexp = /[^\d.-]/g;
                    break;
            }

            modelCtrl.$parsers.push(function (inputValue) {
                if (inputValue) {
                    var textValue = inputValue.toString();
                    var transformedInput = textValue.replace(regexp, '');
                    if (transformedInput !== textValue) {
                        ngModelCtrl.$setViewValue(transformedInput);
                        ngModelCtrl.$render();
                    }
                    return parseFloat(transformedInput);
                }
                return inputValue;
            });
        }
    }
});

app.directive("select3", function ($timeout, $parse) {
    return {
        restrict: 'AC',
        link: function (scope, element, attrs) {
            var options = [],
                el = $(element),
                angularTriggeredChange = false,
                selectOptions = attrs["selectOptions"].split(" in "),
                property = selectOptions[0].split('as');

            var id = property[0].split('.')[1];
            var text = property[1].split('.')[1];
            var optionsObject = selectOptions[1];

            // watch for changes to the defining data model
            scope.$watch(optionsObject, function (n, o) {
                var data = [];
                // format the options for select2 data interface
                for (var i in n) {
                    var obj = { id: n[i][id], text: n[i][text] };
                    data.push(obj);
                }
                el.select2({ data: data });
                // keep local copy of given options
                options = n;
            }, true);
            // watch for changes to the selection data model
            scope.$watch(attrs["selectSelection"], function (n, o) {
                // select2 is indexed by the array position,
                // so we iterate to find the right index
                for (var i in options) {
                    if (options[i][id] === n) {
                        angularTriggeredChange = true;
                        el.val(i).trigger("change");
                    }
                }
            }, true);
            // Watch for changes to the select UI
            el.select2().on("change", function (e) {
                // if the user triggered the change, let angular know
                if (!angularTriggeredChange) {
                    scope.$eval(attrs["selectSelection"] + "='" + options[e.target.value][property] + "'");
                    scope.$digest();
                }
                // if angular triggered the change, then nothing to update
                angularTriggeredChange = false;
            });

        }
    };
});

app.directive('tablecell', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs, autoCtrl) {
            element.bind("keydown", function (event) {
                if (event.which === 38) {
                    if ($(event.currentTarget).parent().parent()[0].rowIndex > 0) {
                        var index = $(event.currentTarget).parent()[0].cellIndex;
                        $(event.currentTarget).parent().parent().prev().find("td:eq(" + index + ")").children().first().focus();
                        event.preventDefault();
                    }
                } else if (event.which == 40) {
                    var index = $(event.currentTarget).parent()[0].cellIndex;
                    $(event.currentTarget).parent().parent().next().find("td:eq(" + index + ")").children().first().focus();
                    event.preventDefault();

                } else if (event.which == 39 || event.which == 13) {

                    if (typeof (event.currentTarget.value) == 'undefined' || event.currentTarget.selectionStart == event.currentTarget.value.length) {
                        scope.$apply(function () {
                            $(event.currentTarget).parent().next().children().first().focus()
                        });
                        event.preventDefault();
                    }
                    //right
                } else if (event.which == 37) {
                    //left
                    if (event.currentTarget.selectionStart == 0) {
                        scope.$apply(function () {
                            $(event.currentTarget).parent().prev().children().first().focus()
                        });
                        event.preventDefault();
                    }
                }
            });
        }
    }
});

//app.directive('onkeyenter', function () {
//    return {
//        restrict: 'A',
//        link: function (scope, element, attrs, autoCtrl) {
//            element.bind("keydown keypress", function (event) {
//                if (event.which === 13) {
//                    scope.$apply(function () {
//                        scope.$eval(attrs.onkeyenter, { $event: event });
//                    });
//                    event.preventDefault();
//                }
//            });
//        }
//    }
//});

app.directive('onkeyenter', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {

            function execute(event) {
                scope.$apply(function () {
                    scope.$eval(attrs.onkeyenter, { $event: event });
                });
            }

            // Standard key handling
            element.on("keydown keypress", function (event) {
                if (event.which === 13 || event.keyCode === 13) {
                    execute(event);
                    event.preventDefault();
                }
            });

            // Android fallback using form submit
            if (!element.closest('form').length) {

                var form = angular.element('<form></form>');

                element.wrap(form);

                var parentForm = element.parent();

                parentForm.on('submit', function (event) {
                    execute(event);
                    event.preventDefault();
                });
            }

        }
    };
});

app.directive('onkeyctrlenter', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs, autoCtrl) {
            element.bind("keydown keypress", function (event) {
                if (event.ctrlKey && event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onkeyctrlenter);
                    });
                    event.preventDefault();
                }
            });
        }
    }
});

app.directive('nextOnEnter', function () {
    return {
        restrict: 'A',
        controller: function ($scope, $element, $attrs) {
            //if (typeof ($attrs.nextOnEnter) != 'undefined' && $attrs.nextOnEnter.length > 0) {
            //    var fooName = 'setFocus' + $attrs.nextOnEnter;
            //    $scope[fooName] = function () {
            //        $element.focus();
            //    }
            //}
        },
        link: function ($scope, selem, attrs) {
            selem.bind('keydown', function (e) {
                var code = e.keyCode || e.which;
                if (code === 13) {
                    e.preventDefault();

                    if (typeof ($attrs.nextOnEnter) != 'undefined' && $attrs.nextOnEnter.length > 0) {
                        var fooName = 'setFocus' + $attrs.nextOnEnter;
                        $scope[fooName]();
                    }
                    else {
                        var pageElems = document.querySelectorAll('input, select, textarea'),
                            elem = e.srcElement,
                            focusNext = false,
                            len = pageElems.length;

                        for (var i = 0; i < len; i++) {
                            var pe = pageElems[i];
                            if (focusNext) {
                                if (pe.style.display !== 'none') {
                                    pe.focus();
                                    break;
                                }
                            } else if (pe === e.srcElement) {
                                focusNext = true;
                            }
                        }
                    }
                }
            });
        }
    }
})

app.directive('focusid', function () {
    return {
        restrict: 'A',
        controller: function ($scope, $element, $attrs) {
            if ($element[0].tagName == 'INPUT' || $element[0].tagName == 'TEXTAREA' || $element[0].tagName == 'BUTTON') {
                if (typeof ($attrs.focusid) == 'string' && $attrs.focusid.startsWith('{{')) {
                    var fooName = 'setFocus' + $scope.$eval($attrs.focusid.replace('{{', '').replace('}}', ''));
                    $scope.$parent[fooName] = function () {
                        $element.focus();
                    };
                }
                else {
                    var fooName = 'setFocus' + $attrs.focusid;

                    if (typeof ($scope.$parent.show_autocomplete) == 'undefined') {
                        $scope[fooName] = function () {
                            $element.focus();
                        }
                    }
                    else {
                        $scope.$parent.$parent[fooName] = function () {
                            $element.focus();
                        }
                    }
                }
            }
        },
    }
});

app.directive('focusonenter', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs, autoCtrl) {

            if (element[0].tagName == 'INPUT') {
                element.bind("keydown keypress", function (event) {
                    if (event.which === 13) {

                        if (typeof (attrs.onshiftenter) != 'undefined' && event.shiftKey) {
                            scope.$apply(function () {
                                scope.$eval(attrs.onshiftenter);
                            });
                        }
                        if (typeof (attrs.focusonenter) != 'undefined') {
                            var fooName = 'setFocus' + attrs.focusonenter + "()";
                            scope.$apply(function () {
                                scope.$eval(fooName);
                            });
                        }
                        event.preventDefault();
                    }
                });
            }
        }
    }
});


var stateProviderConfig = function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('home', {
            url: "/home",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Dashboard.html?v=24',
                    controller: 'DashboardController'
                }
            },
            data: {
                pageTitle: 'Sample Form',
                permission: 'HOMEPAGE'
            }
        }).state('submenu', {
            url: "/submenu",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': null, 'title': '', 'html': '', 'list': [] },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_wh', {
            url: "/submenusub_wh",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_wh' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_mfg', {
            url: "/submenusub_mfg",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_mfg' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_crm', {
            url: "/submenusub_crm",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_crm' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_sales', {
            url: "/submenusub_sales",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_sales' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_fin', {
            url: "/submenusub_fin",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_fin' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_store', {
            url: "/submenusub_store",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_store' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_hr', {
            url: "/submenusub_hr",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_hr' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_anal', {
            url: "/submenusub_anal",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_anal' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_pro', {
            url: "/submenusub_pro",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_pro' },
            data: {
                pageTitle: 'Sample Form'
            }
        }).state('submenusub_set', {
            url: "/submenusub_set",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/menu.html?v=24',
                    controller: 'MenuController'
                }
            },
            params: { 'menu': 'sub_set' },
            data: {
                pageTitle: 'Sample Form'
            }
        })
        .state('LRInbox', {
            url: "/LRInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LRInbox.html?v=24',
                    controller: 'LRInboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INBOX'
            }
        })
        .state('ReorderReport', {
            url: "/ReorderReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Reorder.html?v=24',
                    controller: 'ReOrderController'
                }
            },
            params: { 'type': 'REPORT' },
            data: {
                pageTitle: 'Reorder Report',
                permission: 'REORDER'
            }
        })
        .state('ReorderConfiguration', {
            url: "/ReorderConfiguration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Reorder.html?v=24',
                    controller: 'ReOrderController'
                }
            },
            params: { 'type': 'CONFIG' },
            data: {
                pageTitle: 'Reorder Configuration',
                permission: 'REORDER'
            }
        })
        .state('Intray', {
            url: "/Intray",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Intray.html?v=24',
                    controller: 'IntrayController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INBOX'
            }
        })
        .state('Widgets', {
            url: "/Widgets",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Widgets.html?v=24',
                    controller: 'WidgetsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'WIDGETS'
            }
        })
        .state('ledger', {
            url: "/ledger",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ledger.html?v=24',
                    controller: 'ledgerController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'MANAGELEDGER'
            }
        })

        .state('voucher', {
            url: "/voucher",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/voucher.html?v=24',
                    controller: 'VoucherController'
                }
            },
            params: {
                'voucher':null
            },
            data: {
                pageTitle: 'Lab',
                permission: 'VOUCHERENTRY'
            }
        })
        .state('Courier', {
            url: "/Courier",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Courier.html?v=24',
                    controller: 'CourierController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'COURIER'
            }
        })
        .state('LREntry', {
            url: "/LREntry",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LREntry.html?v=24',
                    controller: 'LREntryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRENTRY'
            }
        })
        .state('StockManagement', {
            url: "/StockManagement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StockManagement.html?v=24',
                    controller: 'StockManagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOCKMARKER'
            }
        })
        .state('StockMarkerAnalyser', {
            url: "/StockMarkerAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StockMarkerAnalyser.html?v=24',
                    controller: 'StockMarkerAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SMANALYSER'
            }
        })
        .state('LRInvoiceItems', {
            url: "/LRInvoiceItems",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LRInvoiceItems.html?v=24',
                    controller: 'LRInvoiceItemsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRPRODUCT'
            }
        })
        .state('LRInvoicePrice', {
            url: "/LRInvoicePrice",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LRInvoicePrice.html?v=24',
                    controller: 'LRInvoicePriceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRPRICER'
            }
        })
        .state('Items', {
            url: "/Items",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Item.html?v=24',
                    controller: 'ItemController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'ITEMMASTER'
            }
        })
        .state('applicationform', {
            url: "/applicationform",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ApplicationForm.html?v=24',
                    controller: 'ApplicationFormController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'Application'
            }
        })
        .state('StockAdj', {
            url: "/StockAdj",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/stockadjustment.html?v=24',
                    controller: 'StockAdjustmentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOCKADJUSTMENT'
            }
        })
        .state('SalesEdit', {
            url: "/SalesEdit",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/salesedit.html?v=24',
                    controller: 'SaleseditController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BILLEDIT'
            }
        })
        .state('Invoice', {
            url: "/Invoice",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Invoice.html?v=24',
                    controller: 'InvoiceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRINVOICE'
            }
        })
        .state('IssueLR', {
            url: "/IssueLR",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/IssueLR.html?v=24',
                    controller: 'IssueLRController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRISSUE'
            }
        })
        .state('trace', {
            url: "/trace",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/trace.html?v=24',
                    controller: 'TraceController'
                }
            },
            data: {
                pageTitle: 'Tracer',
                permission: 'LOGTRACE'
            }
        })
        .state('ReceiptLR', {
            url: "/ReceiptLR",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ReceiptLR.html?v=24',
                    controller: 'ReceiptLRController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LRRECEIPT'
            }
        })
        .state('SaleReturnB2B', {
            url: "/SaleReturnB2B",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesB2B.html?v=24',
                    controller: 'SalesB2BController'
                }
            },
            params: { 'type': 'SALERETURN' },
            data: {
                pageTitle: 'B2B Sales Return',
                permission: 'B2BSALES'
            }
        })
        .state('SalesOfferReturn', {
            url: "/SalesOfferReturn",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesOffer.html?v=24',
                    controller: 'SalesOfferController'
                }
            },
            params: { 'type': 'SALERETURN' },
            data: {
                pageTitle: 'Discount Sales Return',
                permission: 'SALERETURN'
            }
        })
        .state('POBuilder', {
            url: "/POBuilder",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/POBuilder.html?v=24',
                    controller: 'POBuilderController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Purchase Order Builder',
                permission: 'PURCHASEORDER'
            }
        })

        .state('BulkSaleOffer', {
            url: "/BulkSaleOffer",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesOffer.html?v=24',
                    controller: 'SalesOfferController'
                }
            },
            params: { 'type': 'BULKSALE' },
            data: {
                pageTitle: 'Bulk Sale',
                permission: 'BULKSALES'
            }
        })

        .state('EstimateSaleOffer', {
            url: "/EstimateSaleOffer",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesOffer.html?v=24',
                    controller: 'SalesOfferController'
                }
            },
            params: { 'type': 'ESTIMATESALE' },
            data: {
                pageTitle: 'Estimate Sale',
                permission: 'ESTIMATESALE'
            }
        })

        .state('SaleB2BApproval', {
            url: "/SaleB2BApproval",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesB2B.html?v=24',
                    controller: 'SalesB2BController'
                }
            },
            params: { 'type': 'BULKSALE' },
            data: {
                pageTitle: 'Bulk Sale',
                permission: 'BULKSALES'
            }
        })

        .state('SalesOffer', {
            url: "/SalesOffer",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesOffer.html?v=24',
                    controller: 'SalesOfferController'
                }
            },
            params: { 'type': 'SALE' },
            data: {
                pageTitle: 'Discount Sales',
                permission: 'SALES'
            }
        })
        .state('ReportConfiguration', {
            url: "/ReportConfiguration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/reportformatconfig.html?v=24',
                    controller: 'ReportFormatController'
                }
            },
            data: {
                pageTitle: 'Report Configuration',
                permission: 'RPTCONFIGURATION'
            }
        })
        .state('Settlements', {
            url: "/Settlements",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Settlements.html?v=24',
                    controller: 'SettlementsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SETTLEMENT'
            }
        })
        .state('DeliveryRack', {
            url: "/DeliveryRack",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DeliveryRack.html?v=24',
                    controller: 'DeliveryRackController'
                }
            },
            params: { 'type': 'DELIVERYRACK' },
            data: {
                pageTitle: 'Lab',
                permission: 'DELIVERYRACK'
            }
        })

        .state('Delivery', {
            url: "/Delivery",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Delivery.html?v=24',
                    controller: 'DeliveryController'
                }
            },
            params: { 'type': 'DELIVERY' },
            data: {
                pageTitle: 'Lab',
                permission: 'DELIVERY'
            }
        })
        .state('Alteration', {
            url: "/Alteration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Delivery.html?v=24',
                    controller: 'DeliveryController'
                }
            },
            params: { 'type': 'ALTERATION' },
            data: {
                pageTitle: 'Lab',
                permission: 'ALTERATION'
            }
        })
        .state('GiftCouponIssue', {
            url: "/GiftCouponIssue",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Delivery.html?v=24',
                    controller: 'DeliveryController'
                }
            },
            params: { 'type': 'GIFTCOUPONISSUE' },
            data: {
                pageTitle: 'Lab',
                permission: 'GIFTCOUPONISSUE'
            }
        })
        .state('GiftDeliveryIssue', {
            url: "/GiftDeliveryIssue",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Delivery.html?v=24',
                    controller: 'DeliveryController'
                }
            },
            params: { 'type': 'GIFTDELIVERYISSUE' },
            data: {
                pageTitle: 'Lab',
                permission: 'GIFTDELIVERYISSUE'
            }
        })
        .state('GiftDelivery', {
            url: "/GiftDelivery",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Delivery.html?v=24',
                    controller: 'DeliveryController'
                }
            },
            params: { 'type': 'GIFTDELIVERY' },
            data: {
                pageTitle: 'Lab',
                permission: 'GIFTDELIVERY'
            }
        })
        .state('Outward', {
            url: "/Outward",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Outward.html?v=24',
                    controller: 'OutwardsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'OUTWARD'
            }
        })
        .state('Inward', {
            url: "/Inward",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Inward.html?v=24',
                    controller: 'InwardsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INWARD'
            }
        })


        .state('ReceiveGoods', {
            url: "/ReceiveGoods",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ReceiveGoods.html?v=24',
                    controller: 'ReceiveGoodsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RECEIVEGOODS'
            }
        })
        .state('ReturnGoods', {
            url: "/ReturnGoods",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ReturnGoods.html?v=24',
                    controller: 'ReturnGoodsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TRANSFERGOODS'
            }
        })
        .state('ImageGallery', {
            url: "/ImageGallery",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ImageGallery.html?v=24',
                    controller: 'ImageGalleryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'IMAGEGALLERY'
            }
        })
        .state('ProductGallery', {
            url: "/ProductGallery",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductGallery.html?v=24',
                    controller: 'ProductGalleryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRODUCTGALLERY'
            }
        })
        .state('DesignGallery', {
            url: "/DesignGallery",
            views: {
                'container@': {
                    templateUrl: 'clientside/mfg/views/ImageGallery.html?v=24',
                    controller: 'DesignGalleryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DESIGNGALLERY'
            }
        })
        .state('ProductDesign', {
            url: "/ProductDesign",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductDesign.html?v=24',
                    controller: 'ProductDesignController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRODUCTDESIGN'
            }
        })
        .state('ProductDesignMaster', {
            url: "/ProductDesignMaster",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductDesign.html?v=24',
                    controller: 'ProductDesignController'
                }
            },
            params: { 'type': 'DesignMaster' },
            data: {
                pageTitle: 'Lab',
                permission: 'PRODUCTDESIGN'
            }
        })
        .state('PriceList', {
            url: "/PriceList",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PriceList.html?v=24',
                    controller: 'PriceListController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRICELIST'
            }
        })
        .state('PromotionScheme', {
            url: "/PromotionScheme",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Promotion.html?v=24',
                    controller: 'PromotionController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALEPROMOTION'
            }
        })
        .state('PromotionStatus', {
            url: "/PromotionStatus",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PromotionStatus.html?v=24',
                    controller: 'PromotionStatusController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALEPROMOTION'
            }
        })
        .state('Product', {
            url: "/Product",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Product.html?v=24',
                    controller: 'ProductController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRODUCTMASTER'
            }
        })
        .state('supplier', {
            url: "/supplier",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Supplier.html?v=24',
                    controller: 'SupplierController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SUPPLIERMASTER'
            }
        })
        .state('Agent', {
            url: "/Agent",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Agent.html?v=24',
                    controller: 'AgentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'AGENTMASTER'
            }
        })
        .state('Tailor', {
            url: "/Tailor",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Tailor.html?v=24',
                    controller: 'TailorController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TAILORMASTER'
            }
        })
        .state('Transport', {
            url: "/Transport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Transport.html?v=24',
                    controller: 'TransportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TRANSPORTMASTER'
            }
        })
        .state('ServiceCenter', {
            url: "/ServiceCenter",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ServiceCenters.html?v=24',
                    controller: 'ServiceCentersController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SERVICECENTERMASTER'
            }
        })
        .state('Tax', {
            url: "/Tax",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Tax.html?v=24',
                    controller: 'TaxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TAXMASTER'
            }
        })

        .state('barcode', {
            url: "/barcode",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Barcode.html?v=24',
                    controller: 'BarcodeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BARCODEGEN'
            }
        })
        .state('Company', {
            url: "/Company",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Company.html?v=24',
                    controller: 'CompanyController'
                }
            },
            params: { 'type': 'Company' },
            data: {
                pageTitle: 'Lab',
                permission: 'COMPANYMASTER'
            }
        })
        .state('Organisation', {
            url: "/Organisation",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Organisation.html?v=24',
                    controller: 'OrganisationController'
                }
            },
            params: { 'type': 'Organisation' },
            data: {
                pageTitle: 'ORG',
                permission: 'ORGMASTER'
            }
        })
        .state('Employee', {
            url: "/Employee",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Employee.html?v=24',
                    controller: 'EmployeeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEE'
            }
        })
        .state('Referencelist', {
            url: "/Referencelist",
            params: { 'type': 'MASTER', 'productgroup': null },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Referencelist.html?v=24',
                    controller: 'ReferencelistController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REFERENCEMASTER'
            }
        })
        .state('ProductReflist', {
            url: "/ProductReflist",
            params: { 'type': 'PRODUCT' },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Referencelist.html?v=24',
                    controller: 'ReferencelistController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRDREFERENCEMASTER'
            }
        })
        .state('MGRReflist', {
            url: "/MGRReflist",
            params: { 'type': 'MGR' },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Referencelist.html?v=24',
                    controller: 'ReferencelistController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REFERENCEMASTER'
            }
        })
        .state('HRReflist', {
            url: "/HRReflist",
            params: { 'type': 'HR' },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Referencelist.html?v=24',
                    controller: 'ReferencelistController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'HRREFERENCEMASTER'
            }
        })
        .state('Openingcash', {
            url: "/Openingcash",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/OpeningCash.html?v=24',
                    controller: 'OpeningcashController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CASHOPENING'
            }
        })
        .state('Closingcash', {
            url: "/Closingcash",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ClosingCash.html?v=24',
                    controller: 'ClosingcashController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CASHCLOSING'
            }
        })

        .state('PurchaseInsight', {
            url: "/PurchaseInsight",
            params: {
                'source': 'lrinbox',
                'data': { 'lrid': 0, 'poid': 0, 'items': [] }
            },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseInsight.html?v=24',
                    controller: 'PurchaseInsightController'
                }
            },
            data: {
                pageTitle: 'Purchase Insight',
                permission: 'INBOX'
            }
        })

        .state('PurchaseReturn', {
            url: "/PurchaseReturn",
            params: {
                'directlaunch': true,
                'data': {}
            },
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseReturn.html?v=24',
                    controller: 'PurchaseReturnController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PORETURN'
            }
        })

        .state('ProductRequestQueue', {
            url: "/ProductRequestQueue",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductRequestQueue.html?v=24',
                    controller: 'ProductRequestQueueController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POREQUESTQUE'
            },
            params: {
                productgroup: null,
                istextile: null
            }
        })

        .state('TailoringInbox', {
            url: "/TailoringInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/mfg/views/TailoringInbox.html?v=24',
                    controller: 'TailoringInboxController'
                }
            },
            params: {
                'directlaunch': true,
                'source': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TailoringInbox'
            }
        })

        .state('POInbox', {
            url: "/POInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/POInbox.html?v=24',
                    controller: 'POInboxController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POINBOX'
            }
        })

        .state('STOInbox', {
            url: "/STOInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/STOInbox.html?v=24',
                    controller: 'STOInboxController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOINBOX'
            }
        })

        .state('STOInboxScan', {
            url: "/STOInboxScan",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/STOInboxScan.html?v=24',
                    controller: 'STOInboxScanController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOINBOX'
            }
        })

        .state('SOInbox', {
            url: "/SOInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SOInbox.html?v=24',
                    controller: 'SOInboxController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SOINBOX'
            }
        })

        .state('SalesOrder', {
            url: "/SalesOrder",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesOrder.html?v=24',
                    controller: 'SalesOrderController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALEORDER'
            }
        })
        .state('PurchaseOrderGRN', {
            url: "/PurchaseOrderGRN",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseOrderGRN.html?v=24',
                    controller: 'PurchaseOrderGRNController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POGRN'
            }
        })
        .state('LRPOGrn', {
            url: "/LRPOGrn",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LRPOGrn.html?v=24',
                    controller: 'LRPOGrnController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POGRN'
            }
        })
        .state('PurchaseOrder', {
            url: "/PurchaseOrder",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseOrder.html?v=24',
                    controller: 'PurchaseOrderController'
                }
            },
            params: {
                'directlaunch': true,
                'source': '',
                'list': {}
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PURCHASEORDER'
            }
        })
        .state('supplierproductgroups', {
            url: "/supplierproductgroups",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SupplierProductGroups.html?v=24',
                    controller: 'SupplierProductGroupsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SUPPLIERPRODUCTGROUPS'
            }
        })
        .state('Brand', {
            url: "/Brand",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Brand.html?v=24',
                    controller: 'BrandController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BRANDMASTER'
            }
        })
        .state('CustomerDivert', {
            url: "/CustomerDivert",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/CustomerDivert.html?v=24',
                    controller: 'CustomerDivertController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERDIVERT'
            }
        })

        .state('DefaultReferences', {
            url: "/DefaultReferences",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DefaultReferences.html?v=24',
                    controller: 'DefaultReferencesController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('AttributeMapping', {
            url: "/AttributeMapping",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/AttributeMapping.html?v=24',
                    controller: 'AttributeMappingController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'ATTRIBUTEFILTER'
            }
        })

        .state('BulkSale', {
            url: "/BulkSale",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Sales.html?v=24',
                    controller: 'SalesController'
                }
            },
            params: { 'type': 'BULKSALE' },
            data: {
                pageTitle: 'Bulk Sale',
                permission: 'BULKSALES'
            }
        })
        .state('User', {
            url: "/User",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/User.html?v=24',
                    controller: 'UserController'
                }
            },
            data: {
                pageTitle: 'Users',
                permission: 'USERMASTER'
            }
        })

        .state('Customer', {
            url: "/Customer",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Customer.html?v=24',
                    controller: 'CustomerController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERMASTER'
            }
        })
        .state('NumberConfiguration', {
            url: "/NumberConfiguration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/NumberConfiguration.html?v=24',
                    controller: 'NumberConfigurationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'NUMBERCONFIG'
            }
        })

        .state('PurchasePlanner', {
            url: "/PurchasePlanner",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchasePlanner.html?v=24',
                    controller: 'PurchasePlannerController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PURCHASEPLANNER'
            }
        })
        .state('SalesPlanner', {
            url: "/SalesPlanner",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalePlanner.html?v=24',
                    controller: 'SalesPlanAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALESPLANNER'
            }
        })
        .state('IncentiveStatus', {
            url: "/IncentiveStatus",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/IncentiveStatus.html?v=24',
                    controller: 'IncentiveStatusController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INCENTIVESTATUS'
            }
        })
        .state('SplitBarcode', {
            url: "/SplitBarcode",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/splitbarcode.html?v=24',
                    controller: 'SplitBarcodeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SPLITBARCODE'
            }
        })
        .state('PackBarcode', {
            url: "/PackBarcode",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/packbarcode.html?v=24',
                    controller: 'PackBarcodeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'GIFTPACK'
            }
        })
        .state('LastBitBarcode', {
            url: "/LastBitBarcode",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/lastbitbarcode.html?v=24',
                    controller: 'LastBitBarcodeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LASTBIT'
            }
        })
        .state('Price', {
            url: "/Price",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Price.html?v=24',
                    controller: 'PriceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRICINGMASTER'
            }
        })

        .state('DailyExpences', {
            url: "/DailyExpences",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DailyExpences.html?v=24',
                    controller: 'DailyExpencesController'
                }
            },
            data: {
                pageTitle: 'DailyExpences',
                permission: 'DAILYEXPENCES'
            }
        })
        .state('ProductRequestForm', {
            url: "/ProductRequestForm",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductRequestForm.html?v=24',
                    controller: 'ProductRequestFormController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POREQUESTFORM'
            },
            params: {
                productgroup: null,
                istextile: null
            }
        })
        .state('PriceChanger', {
            url: "/PriceChanger",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PriceChanger.html?v=24',
                    controller: 'PriceChangerController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRICECHANGER'
            }
        })
        .state('FestivalDiscount', {
            url: "/FestivalDiscount",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/FestivalDiscount.html?v=24',
                    controller: 'FestivalDiscountController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FESTIVALDISCOUNT'
            }
        })
        .state('Refill', {
            url: "/Refill",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/RefillSales.html?v=24',
                    controller: 'RefillSalesController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REFILL'
            }
        })
        .state('RefillBox', {
            url: "/RefillBox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/RefillBox.html?v=24',
                    controller: 'RefillBoxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REFILLBOX'
            }
        })
        .state('SalaryManagement', {
            url: "/SalaryManagement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/salarymanagement.html?v=24',
                    controller: 'salarymanagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALARYMGMT'
            }
        })
        .state('TagetManagement', {
            url: "/TagetManagement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/targetmanagement.html?v=24',
                    controller: 'targetmanagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TARGETMGMT'
            }
        })
        .state('EmployeeIncharge', {
            url: "/EmployeeIncharge",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/EmployeeIncharge.html?v=24',
                    controller: 'EmployeeInchargeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEEINCHARGE'
            }
        })
        .state('PurchaseBudget', {
            url: "/PurchaseBudget",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseBudget.html?v=24',
                    controller: 'PurchaseBudgetController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PURCHASEBUDGET'
            }
        })
        .state('IncrementManagement', {
            url: "/IncrementManagement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/incrementmanagement.html?v=24',
                    controller: 'incrementmanagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INCREMENTMGMT'
            }
        })


        .state('SalaryStructure', {
            url: "/SalaryStructure",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/salarystructure.html?v=24',
                    controller: 'salarystructureController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALARYSTRUCTURE'
            }
        })
        .state('holiday', {
            url: "/holiday",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/holiday.html?v=24',
                    controller: 'holidaycontroller'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'HOLIDAYMASTER'
            }
        })

        .state('EmployeeLeave', {
            url: "/EmployeeLeave",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/EmployeeLeave.html?v=24',
                    controller: 'EmployeeLeaveController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEELEAVE'
            }
        })

        .state('EmployeeSalary', {
            url: "/EmployeeSalary",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/EmployeeSalary.html?v=24',
                    controller: 'EmployeeSalaryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PAYROLL'
            }
        })



        .state('PayrollProcessing', {
            url: "/PayrollProcessing",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/payroll.html?v=24',
                    controller: 'PayrollController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PAYROLL'
            }
        })

        .state('BonusProcessing', {
            url: "/BonusProcessing",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/BonusProcessing.html?v=24',
                    controller: 'BonusProcessingController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOUNSPAYROLL'
            }
        })

        .state('AdditionalPayroll', {
            url: "/AdditionalPayroll",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/AdditionalPayroll.html?v=24',
                    controller: 'AdditionalPayrollController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'ADDITIONALPAYROLL'
            }
        })
        .state('PaySlip', {
            url: "/PaySlip",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/payslip.html?v=24',
                    controller: 'PayslipController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PAYSLIP'
            }
        })
        .state('SalaryDeduction', {
            url: "/SalaryDeduction",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/employeeadvance.html?v=24',
                    controller: 'EmployeeAdvanceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALARYDEDUCTION'
            }
        })

        .state('Advance', {
            url: "/Advance",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Advance.html?v=24',
                    controller: 'AdvanceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPADVANCE'
            }
        })

        .state('ShiftAllocation', {
            url: "/ShiftAllocation",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ShiftAllocation.html?v=24',
                    controller: 'ShiftAllocationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SHIFTALLOCATION'
            }
        })

        .state('formgeneration', {
            url: "/formgeneration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/formgeneration.html?v=24',
                    controller: 'FormGenerationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FORMGENERATION'
            }
        })

        .state('empinout', {
            url: "/empinout",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Employeeinout.html?v=24',
                    controller: 'EmployeeinoutController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEEINOUT'
            }
        })

        .state('ItemSearch', {
            url: "/ItemSearch",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ItemSearch.html?v=24',
                    controller: 'ItemSearchController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SEARCHITEM'
            }
        })

        .state('supplierpayment', {
            url: "/supplierpayment",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SupplierPayment.html?v=24',
                    controller: 'SupplierPaymentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SUPPLIERPAYMENT'
            }
        })

        .state('customercreditnote', {
            url: "/customercreditnote",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CustomerCreditNote.html?v=24',
                    controller: 'CustomerCreditNoteController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERCREDITNOTE'
            }
        })


        .state('customerpayment', {
            url: "/customerpayment",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CustomerPayment.html?v=24',
                    controller: 'CustomerPaymentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERPAYMENT'
            }
        })
        .state('payrollreport', {
            url: "/payrollreport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PayrollReport.html?v=24',
                    controller: 'PayrollReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTPAYROLL'
            }
        })

        .state('MissingStock', {
            url: "/MissingStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/MissingStock.html?v=24',
                    controller: 'MissingStockController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOCKENTRY'
            }
        })

        .state('RetailReport', {
            url: "/RetailReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/RetailReport.html?v=24',
                    controller: 'RetailReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTRETAIL'
            }
        })

        .state('WareHouseReport', {
            url: "/WareHouseReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/WareHouseReport.html?v=24',
                    controller: 'WareHouseReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTWAREHOUSE'
            }
        })
        .state('HRReport', {
            url: "/HRReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/HRReport.html?v=24',
                    controller: 'HRReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTHR'
            }
        })

        .state('StockAnalyser', {
            url: "/StockAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StockAnalyser.html?v=24',
                    controller: 'StockAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOCKANALYSER'
            }
        })

        .state('StockStressAnalyser', {
            url: "/StockStressAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StockStressAnalyser.html?v=24',
                    controller: 'StockStressAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STRESSANALYSER'
            }
        })

        .state('SalesConversionAnalyser', {
            url: "/SalesConversionAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesConversionAnalyser.html?v=24',
                    controller: 'SalesConversionAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REVENUEGENERATION'
            }
        })

        .state('StockPosition', {
            url: "/StockPosition",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StockPositionAnalyser.html?v=24',
                    controller: 'StockPositionController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOCKPOSITION'
            }
        })


        .state('SalesComparer', {
            url: "/SalesComparer",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesComparAnalyser.html?v=24',
                    controller: 'SalesCompareAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALESCOMPARER'
            }
        })
        .state('SalesAnalyser', {
            url: "/SalesAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesAnalyser.html?v=24',
                    controller: 'SalesAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALESANALYSER'
            }
        })
        .state('SalesVsPurchase', {
            url: "/SalesVsPurchase",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesVsPurchase.html?v=24',
                    controller: 'SalesVsPurchaseController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALESVSPURCHASE'
            }
        })
        .state('SalesVsStock', {
            url: "/SalesVsStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesVsStockAnalyser.html?v=24',
                    controller: 'SalesVsStockController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SALESVSSTOCK'
            }
        })
        .state('PRAnalyser', {
            url: "/PRAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseReturnAnalyser.html?v=24',
                    controller: 'PurchaseReturnAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRANALYSER'
            }
        })
        .state('PurchaseAnalyser', {
            url: "/PurchaseAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PurchaseAnalyser.html?v=24',
                    controller: 'PurchaseAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PURCHASEANALYSER'
            }
        })
        .state('ProductMovement', {
            url: "/ProductMovement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductMovement.html?v=24',
                    controller: 'ProductMovementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTPRODUCTMOVEMENT'
            }
        })
        .state('tallyintegration', {
            url: "/tallyintegration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TallyIntegration.html?v=24',
                    controller: 'TallyIntegrationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TALLYINTEGRATION'
            }
        })
        .state('HRDashboard', {
            url: "/HRDashboard",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/HRDashboard.html?v=24',
                    controller: 'HRDashboardController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTHRDASHBOARD'
            }
        })

        .state('EmpSticker', {
            url: "/EmpSticker",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/EmployeeStickerPrint.html?v=24',
                    controller: 'EmployeeStickerController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEESTICKER'
            }
        })

        .state('ProductGroup', {
            url: "/ProductGroup",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ProductGroup.html?v=24',
                    controller: 'ProductGroupController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRODUCTMASTER'
            }
        })
        .state('SaleB2B', {
            url: "/SaleB2B",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesB2B.html?v=24',
                    controller: 'SalesB2BController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'B2BSALES'
            }
        })
        .state('SalesInbox', {
            url: "/SalesInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SalesInbox.html?v=24',
                    controller: 'SalesInboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'B2BINBOX'
            }
        })
        .state('EcomInbox', {
            url: "/EcomInbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/EcomInbox.html?v=24',
                    controller: 'EcomInboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'ECOMINBOX'
            }
        })
        .state('Dumping', {
            url: "/Dumping",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Dumping.html?v=24',
                    controller: 'DumpingController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DUMPING'
            }
        })
        .state('NonTextileSale', {
            url: "/NonTextileSale",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/NonTextileSale.html?v=24',
                    controller: 'NonTextileSaleController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POSSALES'
            }
        })
        .state('TallyConfig', {
            url: "/TallyConfig",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TallyConfig.html?v=24',
                    controller: 'CompanyController'
                }
            },
            params: { 'type': 'TallyConfig' },
            data: {
                pageTitle: 'Lab',
                permission: 'TALLYCONFIG'
            }
        })
        .state('ApplicationSettings', {
            url: "/ApplicationSettings",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ApplicationSettings.html?v=24',
                    controller: 'ApplicationSettingsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'APPLICATIONSETTING'
            }
        })

        .state('Coupon', {
            url: "/Coupon",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CouponScheme.html?v=24',
                    controller: 'CouponSchemeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'COUPONS'
            }
        })


        .state('GiftVoucher', {
            url: "/GiftVoucher",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/GiftVoucher.html?v=24',
                    controller: 'GiftVoucherController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'GIFTVOUCHER'
            }
        })

        .state('Reports', {
            url: "/Reports",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Reports.html?v=24',
                    controller: 'ReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTWAREHOUSE'
            }
        })

        .state('B2BSALES', {
            url: "/B2BSALES",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'B2B' },

            data: {
                pageTitle: 'SalesReports',
                permission: 'B2BSALESREPORT'
            }

        })

        .state('SALESREPORT', {
            url: "/SALESREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESREPORT' },

            data: {
                pageTitle: 'SalesReports',
                permission: 'SALESREPORT'
            }

        })
        .state('DAYSALESREPORT', {
            url: "/DAYSALESREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesDaySummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'DAYSALESREPORT' },

            data: {
                pageTitle: 'DAYSALESREPORT',
                permission: 'DAYSALESREPORT'
            }
        })
        .state('DAYSUMMARY', {
            url: "/DAYSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/DaySummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'DAYSUMMARY' },

            data: {
                pageTitle: 'DaySummary',
                permission: 'MOBILEDAYBOOK'
            }
        })
        .state('EMIStatement', {
            url: "/EMIStatement",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/EMIStatement.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'EMIStatement' },

            data: {
                pageTitle: 'EMIStatement',
                permission: 'MOBILEEMIREPORT'
            }
        })
        .state('BillItemDetails', {
            url: "/BillItemDetails",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BillItemDetails.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'BillItemDetails' },

            data: {
                pageTitle: 'BillItemDetails',
                permission: 'MOBILEBILLREPORT'
            }
        })
        .state('SchemeDetails', {
            url: "/SchemeDetails",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SchemeDetailsReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SchemeDetails' },

            data: {
                pageTitle: 'SchemeDetails',
                permission: 'MOBILESCHEMEREPORT'
            }
        })
        .state('ECStatement', {
            url: "/ECStatement",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/ECStatement.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ECStatement' },

            data: {
                pageTitle: 'ECStatement',
                permission: 'MOBILERECHARGEREPORT'
            }
        })
        .state('SALESSNAPSHOT', {
            url: "/SALESSNAPSHOT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesSnapshot.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESSNAPSHOT' },

            data: {
                pageTitle: 'SalesSnapshot',
                permission: 'SNAPSHOTSALESREPORT'
            }

        })
        .state('TEXTDAYSUMMARY', {
            url: "/TEXTDAYSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/TextDaySummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'TEXTDAYSUMMARY' },

            data: {
                pageTitle: 'Text Day Summary',
                permission: 'TEXTDAYSUMMARY'
            }

        })
        .state('CSALESREPORT', {
            url: "/CSALESREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CreditSalesReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CSALESREPORT' },

            data: {
                pageTitle: 'SalesReports'
            }

        })
        .state('SALESSUMMARY', {
            url: "/SALESSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesSummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESSUMMARY' },

            data: {
                pageTitle: 'SalesReports'
            }

        })

        .state('UNSETTLED', {
            url: "/UNSETTLED",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/UnsettledBills.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'UNSETTLED' },

            data: {
                pageTitle: 'SalesReports',
                permission: 'UNSETTLEDSALESREPORT'
            }

        })

        .state('SplitBarcodeReport', {
            url: "/SplitBarcodeReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Splitbarcodereport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SPLITBARCODEREP' },
            data: {
                pageTitle: 'Split barcode Report',
                permission: 'SPLITBARCODE'
            }
        })
        .state('SALESMARGIN', {
            url: "/SALESMARGIN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesMarginReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESMARGIN' },

            data: {
                pageTitle: 'Sales Margin Reports',
                permission: 'MARGINSALESREPORT'
            }

        })

        .state('STOCKDEPRECIATION', {
            url: "/STOCKDEPRECIATION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockDepreciation.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'STOCKDEPRECIATION' },
            data: {
                pageTitle: 'Stock Depreciation',
                permission: 'STOCKDEPRECIATION'
            }
        })

        .state('STOCKPURCHASESALE', {
            url: "/STOCKPURCHASESALE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesVsPurchaseVsStock.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'STOCKPURCHASESALE' },
            data: {
                pageTitle: 'Stock Vs Purchase Vs Sales',
                permission: 'STOCKPURCHASESALE'
            }
        })
  
        .state('ASONSTOCKTRANSACTION', {
            url: "/ASONSTOCKTRANSACTION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AsOnStockTransaction.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'ASONSTOCKTRANSACTION' },
            data: {
                pageTitle: 'Ason Date Stock transaction',
                permission: 'STOCKTRANSACTIONREPORT'
            }
        })

        .state('ASONSTOCK', {
            url: "/ASONSTOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AsOnStockNew.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'ASONSTOCK' },

            data: {
                pageTitle: 'Ason Date Stock',
                permission: 'WHSTOCKASONREPORT'
            }

        })

        .state('BILLDETAILS', {
            url: "/BILLDETAILS",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BillDetails.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'BILLDETAILS' },

            data: {
                pageTitle: 'SalesReports'
            }

        })
        .state('PRODUCTREPORT', {
            url: "/PRODUCTREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/ProductWise.html?v=24',
                    controller: 'RetReportController'
                }
            },
            data: {
                pageTitle: 'PRODUCTWISESALES'
            }

        })

        .state('SALESRETURN', {
            url: "/SALESRETURN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesReturn.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESRETURN' },
            data: {
                pageTitle: 'SALESRETURN'
            }

        })

        .state('ADDITIONALDISC', {
            url: "/ADDITIONALDISC",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AdditionalDiscount.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ADDITIONALDISC' },
            data: {
                pageTitle: 'ADDITIONALDISC',
                permission: 'SALESBILLERREPORT'
            }

        })

        .state('ADDITIONALCASHDISC', {
            url: "/ADDITIONALCASHDISC",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CashierWiseAdDiscount.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ADDITIONALCASHDISC' },
            data: {
                pageTitle: 'ADDITIONALCASHDISC',
                permission: 'SALESCASHIERREPORT'
            }

        })

        .state('SETTLEMENT', {
            url: "/SETTLEMENT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Settlement.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENT' },
            data: {
                pageTitle: 'SETTLEMENT',
                permission: 'SETTLEMENTREPORT'
            }
        })

        .state('CASHIERWISESETTLEMENT', {
            url: "/CASHIERWISESETTLEMENT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CashierWiseSettlement.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CASHIERWISESETTLEMENT' },
            data: {
                pageTitle: 'CASHIERWISESETTLEMENT',
                permission: 'SETTLEMENTREPORT'
            }
        })
        .state('SWIPINGMACH', {
            url: "/SWIPINGMACH",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SettlementSmachine.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENT' },
            data: {
                pageTitle: 'MOBILESWIPEMACHINEREPORT',
                permission: 'MOBILESWIPEMACHINEREPORT'
            }
        })

        .state('SETTLEMENTDETAIL', {
            url: "/SETTLEMENTDETAIL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SettlementDetail.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENTDETAIL' },
            data: {
                pageTitle: 'SETTLEMENTDETAIL',
                permission: 'SETTLEMENTREPORT'
            }

        })

        .state('CANCELLEDBILL', {
            url: "/CANCELLEDBILL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CancelledBill.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CANCELLEDBILL' },

            data: {
                pageTitle: 'CANCELLEDBILL',
                permission: 'SALESCANCELLEDREPORT'
            }

        })

        .state('BULKBILL', {
            url: "/BULKBILL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BulkBill.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'BULKBILL' },
            data: {
                pageTitle: 'BULKBILL',
                permission: 'BULKBILLREPORT'
            }

        })
        .state('DELIVERYREPORT', {
            url: "/DELIVERYREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/DeliveryReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'DELIVERYREPORT' },
            data: {
                pageTitle: 'DELIVERYREPORT',
                permission: 'DELIVERYREPORT'
            }

        })
        .state('ALTERATIONREPORT', {
            url: "/ALTERATIONREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AlterationReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ALTERATIONREPORT' },
            data: {
                pageTitle: 'ALTERATIONREPORT',
                permission: 'ALTERATIONREPORT'
            }

        })

        .state('ESTIMATEBILLREPORT', {
            url: "/ESTIMATEBILLREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/EstimateBill.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ESTIMATEBILLREPORT' },
            data: {
                pageTitle: 'ESTIMATEBILLREPORT',
                permission: 'ESTIMATEBILLREPORT'
            }

        })

        .state('DELIVERY', {
            url: "/DELIVERY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/DeliveryStatus.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'DELIVERY' },

            data: {
                pageTitle: 'DELIVERY'
            }

        })
        .state('B2BTAX', {
            url: "/B2BTAX",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesTax.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'B2B' },

            data: {
                pageTitle: 'SALESTAX',
                permission: 'B2BSALESTAXREPORT'
            }

        })
        .state('SALESTAX', {
            url: "/SALESTAX",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesTax.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESTAX' },

            data: {
                pageTitle: 'SALESTAX',
                permission: 'SALESTAXREPORT'
            }

        })

        .state('SALESTAXSUMMARY', {
            url: "/SALESTAXSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesTaxSummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESTAXSUMMARY' },

            data: {
                pageTitle: 'SALESTAXSUMMARY',
                permission: 'SALESTAXSUMMARYREPORT'
            }

        })

        .state('SALESSPLIT', {
            url: "/SALESSPLIT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesTaxSplitup.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESSPLIT' },

            data: {
                pageTitle: 'SALESSPLIT',
                permission: 'SALESTAXREPORT'
            }

        })

        .state('B2BSALESSPLIT', {
            url: "/B2BSALESSPLIT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/B2BSalesTaxSplitup.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'B2B' },

            data: {
                pageTitle: 'SALESSPLIT',
                permission: 'SALESTAXREPORT'
            }

        })

        .state('SALEREGISTER', {
            url: "/SALEREGISTER",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InvoiceWiseSales.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALEREGISTER' },

            data: {
                pageTitle: 'SALEREGISTER',
                permission: 'SALESREPORT'
            }

        })
        .state('B2BSALESHSN', {
            url: "/B2BSALESHSN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesHSN.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'B2B' },

            data: {
                pageTitle: 'B2b SALES HSN',
                permission: 'B2BSALESHSNREPORT'
            }

        })

        .state('SALESHSN', {
            url: "/SALESHSN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesHSN.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESHSN' },

            data: {
                pageTitle: 'SALESHSN',
                permission: 'SALESHSNREPORT'
            }

        })
        .state('SALESSPLITUP', {
            url: "/SALESSPLITUP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesSplitUp.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESSPLITUP' },
            data: {
                pageTitle: 'SALESSPLITUP',
                permission: 'SALESTAXREPORT'
            }
        })
        .state('GSTRREPORT', {
            url: "/GSTRREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/GSTRREPORT.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'GSTRREPORT' },
            data: {
                pageTitle: 'GSTRREPORT',
                permission: 'GSTRREPORT'
            }
        })
        .state('STOCK', {
            url: "/STOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Stock.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'STOCK' },
            data: {
                pageTitle: 'STOCK',
                permission: 'WHSTOCKREPORT'
            }

        })

        .state('STOCKMOVEMENT', {
            url: "/STOCKMOVEMENT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockMovement.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'STOCKMOVEMENT' },
            data: {
                pageTitle: 'STOCKMOVEMENT',
                permission: 'WHSTOCKLOCATIONWISEREPORT'
            }

        })

        .state('SALESMANREP', {
            url: "/SALESMANREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesMan.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESMANREP' },
            data: {
                pageTitle: 'SALESMAN',
                permission: 'SALESMANREPORT'
            }

        })
        .state('SALESMANDETREP', {
            url: "/SALESMANDETREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesManDetail.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESMANDETREP' },
            data: {
                pageTitle: 'SALESMAN',
                permission: 'SALESMANREPORT'
            }

        })

        .state('RETAILSTOCK', {
            url: "/RETAILSTOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/RetailStockReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'RETAILSTOCK' },
            data: {
                pageTitle: 'RETAILSTOCK',
                permission: 'RETAILSTOCKREPORT'
            }

        })

        .state('RETAILSHELFSTOCK', {
            url: "/RETAILSHELFSTOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/RetailStockShelfReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'RETAILSHELFSTOCK' },
            data: {
                pageTitle: 'RETAILSHELFSTOCK',
                permission: 'RETAILSTOCKREPORT'
            }

        })

        .state('RETAILSTOCKAGE', {
            url: "/RETAILSTOCKAGE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/RetailStockAgeReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'RETAILSTOCKAGE' },
            data: {
                pageTitle: 'RETAILSTOCKAGE',
                permission: 'RETAILSTOCKREPORT'
            }

        })

        .state('INWARDREP', {
            url: "/INWARDREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Inward.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'INWARDREP' },
            data: {
                pageTitle: 'INWARDREP'
            }

        })

        .state('INWARDDETAIL', {
            url: "/INWARDDETAIL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InwardItems.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'INWARDDETAIL' },
            data: {
                pageTitle: 'INWARDDETAIL'
            }

        })

        .state('CUSTOMERPAYMENTREP', {
            url: "/CUSTOMERPAYMENTREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerPayment.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CUSTOMERPAYMENTREP' },
            data: {
                pageTitle: 'CUSTOMERPAYMENTREP',
                permission: 'CUSTOMERPAYMENTREPORT'
            }

        })

        .state('CreditNoteReport', {
            url: "/CreditNoteReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerCreditNote.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CreditNoteReport' },
            data: {
                pageTitle: 'CreditNoteReport',
                permission: 'CUSTOMERCREDITNOTEREPORT'
            }

        })
        .state('SettlementCouponIssueReport', {
            url: "/SettlementCouponIssueReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CouponIssueReport.html?v=24',
                    controller: 'CouponSchemeController'
                }
            },
            params: { 'type': 'SettlementCouponIssueReport' },
            data: {
                pageTitle: 'SettlementCouponIssueReport',
                permission: 'SETTLEMENTCOUPONS'
            }

        })
        .state('CouponIssueReport', {
            url: "/CouponIssueReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CouponIssueReport.html?v=24',
                    controller: 'CouponSchemeController'
                }
            },
            params: { 'type': 'CouponIssueReport' },
            data: {
                pageTitle: 'CouponIssueReport',
                permission: 'COUPONS'
            }

        })
        .state('CouponReport', {
            url: "/CouponReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CouponReport.html?v=24',
                    controller: 'CouponSchemeController'
                }
            },
            params: { 'type': 'CouponReport' },
            data: {
                pageTitle: 'CouponReport',
                permission: 'COUPONS'
            }

        })
        .state('VoucherReport', {
            url: "/VoucherReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/GiftVoucherReport.html?v=24',
                    controller: 'CouponSchemeController'
                }
            },
            params: { 'type': 'VoucherReport' },
            data: {
                pageTitle: 'VoucherReport',
                permission: 'GIFTVOUCHER'
            }

        })
        .state('GiftIssueReport', {
            url: "/GiftIssueReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/GiftIssueReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'GiftIssueReport' },
            data: {
                pageTitle: 'GiftIssueReport',
                permission: 'GIFTISSUEREPORT'
            }

        })
        .state('RewardReport', {
            url: "/RewardReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/RewardReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'RewardReport' },
            data: {
                pageTitle: 'RewardReport',
                permission: 'SALESREPORT'
            }

        })

        .state('MISSINGSTOCKREP', {
            url: "/MISSINGSTOCKREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/MissingStockReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'MISSINGSTOCKREP' },

            data: {
                pageTitle: 'SalesReports',
                permission: 'RETAILDIRECTSTOCKREPORT'
            }

        })

        .state('SALESANALYSIS', {
            url: "/SALESANALYSIS",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesAnalysis.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESANALYSIS' },
            data: {
                pageTitle: 'SALESANALYSIS',
                permission: 'SALESANALYSISREPORT'
            }

        })

        .state('SALEVSSET', {
            url: "/SALEVSSET",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesVsSettlement.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALEVSSET' },
            data: {
                pageTitle: 'SALEVSSET',
                permission: 'SALESVSSETTLEMENTREPORT'
            }
        })
        .state('SALEVSSTOCK', {
            url: "/SALEVSSTOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesVsStock.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALESVSSTOCK' },
            data: {
                pageTitle: 'SALESVSSTOCK',
                permission: 'SALESVSSTOCKREPORT'
            }
        })
        .state('INCSECTION', {
            url: "/INCSECTION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/IncentiveSection.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'INCSECTION' },
            data: {
                pageTitle: 'INWARDREP'
            }

        })

        .state('INCEMPLOYEE', {
            url: "/INCEMPLOYEE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/IncentiveEmploye.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'INCEMPLOYEE' },
            data: {
                pageTitle: 'INCEMPLOYEE'
            }

        })
        .state('EMPLOYEEDET', {
            url: "/EMPLOYEEDET",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/EmployeeDetail.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'EMPLOYEEDET' },
            data: {
                pageTitle: 'EMPLOYEEDET'
            }

        })

        .state('LRREPORT', {
            url: "/LRREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/LRReport.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'LRREPORT', 'Search': false, 'Command': null, 'fromdate': null, 'todate': null },
            data: {
                pageTitle: 'LRREPORT',
                permission: 'LRREPORT'
            }

        })

        .state('LRINVOICE', {
            url: "/LRINVOICE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Invoice.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'LRINVOICE', 'Search': false, 'Command': null, 'fromdate': null, 'todate': null },

            data: {
                pageTitle: 'LRINVOICE',
                permission: 'LRINVOICEREPORT'
            }

        })

        .state('LRINVOICEDETAIL', {
            url: "/LRINVOICEDETAIL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InvoiceDetail.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'LRINVOICEDETAIL', 'Search': false, 'Command': null, 'fromdate': null, 'todate': null },

            data: {
                pageTitle: 'LRINVOICEDETAIL',
                permission: 'LRINVOICEREPORT'
            }

        })
        .state('SETTLEMENTSUMMARY', {
            url: "/SETTLEMENTSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SettlementSummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENTSUMMARY' },
            data: {
                pageTitle: 'SETTLEMENTSUMMARY',
                permission: 'SETTLEMENTREPORT'
            }
        })
        .state('SETTLEMENTDAYSUMMARY', {
            url: "/SETTLEMENTDAYSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SettlementDaySummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENTDAYSUMMARY' },
            data: {
                pageTitle: 'SETTLEMENTDAYSUMMARY',
                permission: 'SETTLEMENTREPORT'
            }
        })
        .state('SETTLEMENTRECONCILATION', {
            url: "/SETTLEMENTRECONCILATION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SettlementReconcilation.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SETTLEMENTRECONCILATION' },
            data: {
                pageTitle: 'SETTLEMENTRECONCILATION',
                permission: 'SETTLEMENTREPORT'
            }
        })
        .state('CREDITSALESUMMARY', {
            url: "/CREDITSALESUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CreditSaleSummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'CREDITSALESUMMARY' },
            data: {
                pageTitle: 'CREDITSALESUMMARY',
                permission: 'SETTLEMENTREPORT'
            }
        })
        .state('ADVANCESUMMARY', {
            url: "/ADVANCESUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerAdvanceSummary.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'ADVANCESUMMARY' },
            data: {
                pageTitle: 'ADVANCESUMMARY',
                permission: 'SETTLEMENTREPORT'
            }

        })
        .state('WHInvoicePO', {
            url: "/WHInvoicePO",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InvoiceVsPurchaseOrder.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'WHInvoicePO' },
            data: {
                pageTitle: 'WHInvoicePO',
                permission: 'PURCHASEREPORT'
            }
        })
        .state('WHEntry', {
            url: "/WHEntry",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/WHEntry.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'WHEntry' },
            data: {
                pageTitle: 'WHEntry',
                permission: 'PURCHASEREPORT'
            }
        })
        .state('PURCHASE', {
            url: "/PURCHASE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Purchase.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASE' },
            data: {
                pageTitle: 'PURCHASE',
                permission: 'PURCHASEREPORT'
            }

        })
        .state('PURCHASEBARCODE', {
            url: "/PURCHASEBARCODE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BarcodeDetail.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASE DETAILS' },
            data: {
                pageTitle: 'PURCHASE DETAILS',
                permission: 'PURCHASEREPORT'
            }

        })
        .state('PURCHASESECTION', {
            url: "/PURCHASESECTION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseSectionwise.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASESECTION' },
            data: {
                pageTitle: 'PURCHASESECTION',
                permission: 'PURCHASESECTIONREPORT'
            }

        })
        .state('PURCHASERETURNSECTION', {
            url: "/PURCHASERETURNSECTION",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseRetSectionwise.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASERETURNSECTION' },
            data: {
                pageTitle: 'PURCHASERETURNSECTION',
                permission: 'PURCHASERETURNSECTIONREPORT'
            }

        })
        .state('PURCHASEITEMS', {
            url: "/PURCHASEITEMS",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseItems.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASEITEMS' },
            data: {
                pageTitle: 'PURCHASEITEMS',
                permission: 'PURCHASEHSNREPORT'
            }
        })
        .state('PURCHASETAXSUMMARY', {
            url: "/PURCHASETAXSUMMARY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseTaxSummary.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASETAX' },
            data: {
                pageTitle: 'PURCHASETAX',
                permission: 'PURCHASETAXREPORT'
            }
        })
        .state('PURCHASETAX', {
            url: "/PURCHASETAX",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseTax.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASETAX' },
            data: {
                pageTitle: 'PURCHASETAX',
                permission: 'PURCHASETAXREPORT'
            }
        })
        .state('PURCHASEHSN', {
            url: "/PURCHASEHSN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseHSN.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASEHSN' },
            data: {
                pageTitle: 'PURCHASEHSN',
                permission: 'PURCHASEHSNREPORT'
            }

        })
        .state('supplierbillpendings', {
            url: "/supplierbillpendings",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SupplierPaymentReport.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'supplierbillpendings' },
            data: {
                pageTitle: 'Supplier Pending Bills',
                permission: 'SUPPLIERPENDING'
            }
        })
        .state('transportbillpendings', {
            url: "/transportbillpendings",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/TransportPaymentReport.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'transportbillpendings' },
            data: {
                pageTitle: 'Transport Pending Bills',
                permission: 'SUPPLIERPENDING'
            }
        })
        .state('supplierpaymentrep', {
            url: "/supplierpaymentrep",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SupplierPayment.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'supplierpaymentrep' },
            data: {
                pageTitle: 'Supplier Payment',
                permission: 'SUPPLIERPAYMENTREPORT'
            }
        })
        .state('transportpaymentrep', {
            url: "/transportpaymentrep",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/TransportPayment.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'transportpaymentrep' },
            data: {
                pageTitle: 'Transport Payment',
                permission: 'SUPPLIERPAYMENTREPORT'
            }

        })
        .state('PRODUCTCREATE', {
            url: "/PRODUCTCREATE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/NewProductsReports.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PRODUCTCREATE' },
            data: {
                pageTitle: 'PRODUCTCREATE',
                permission: 'PRODUCTMASTER'
            }

        })
        .state('PURCHASERETURNAUDIT', {
            url: "/PURCHASERETURNAUDIT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseReturnAudit.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASERETURNAUDIT' },
            data: {
                pageTitle: 'PURCHASERETURNAUDIT',
                permission: 'PURCHASERETURNAUDITREPORT'
            }

        })

        .state('SUPPCREATE', {
            url: "/SUPPCREATE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/NewSuppliersReports.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'SUPPCREATE' },
            data: {
                pageTitle: 'SUPPCREATE',
                permission: 'SUPPLIERMASTER'
            }

        })

        .state('AGENTCREATE', {
            url: "/AGENTCREATE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/NewAgents.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'AGENTCREATE' },
            data: {
                pageTitle: 'AGENTCREATE',
                permission: 'AGENTMASTER'
            }

        })

        .state('TAXCREATE', {
            url: "/TAXCREATE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/NewTax.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'TAXCREATE' },
            data: {
                pageTitle: 'TAXCREATE',
                permission: 'TAXMASTER'
            }

        })


        .state('PURCHASERETURN', {
            url: "/PURCHASERETURN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseReturn.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASERETURN', 'Search': false, 'Command': null, 'fromdate': null, 'todate': null },
            data: {
                pageTitle: 'PURCHASERETURN',
                permission: 'PURCHASERETURNREPORT'
            }
        })

        .state('PURCHASERETURNCANCEL', {
            url: "/PURCHASERETURNCANCEL",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseRetCancelled.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'PURCHASERETURNCANCEL' },
            data: {
                pageTitle: 'PURCHASERETURNCANCEL',
                permission: 'PURCHASERETURNCANCELREPORT'
            }
        })

        .state('OUTWARD', {
            url: "/OUTWARD",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/Outward.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'OUTWARD' },
            data: {
                pageTitle: 'OUTWARD',
                permission: 'OUTWARDREPORT'
            }
        })
        .state('JOBOUTWARD', {
            url: "/JOBOUTWARD",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/JobOutward.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'JOBOUTWARD' },
            data: {
                pageTitle: 'JOBOUTWARD',
                permission: 'JOBOUTWARDREPORT'
            }
        })
        .state('JOBINWARD', {
            url: "/JOBINWARD",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/JobInward.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'JOBINWARD' },
            data: {
                pageTitle: 'JOBINWARD',
                permission: 'JOBINWARDREPORT'
            }
        })
        .state('STOCKANALYSIS', {
            url: "/STOCKANALYSIS",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockAnalysis.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'STOCKANALYSIS' },
            data: {
                pageTitle: 'STOCKANALYSIS',
                permission: 'WHSTOCKANALYSISREPORT'
            }
        })

        .state('RETSTOCKANALYSIS', {
            url: "/RETSTOCKANALYSIS",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/RetailStockAnalysis.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'RETSTOCKANALYSIS' },
            data: {
                pageTitle: 'RETSTOCKANALYSIS',
                permission: 'RETAILSTOCKANALYSISREPORT'
            }
        })
        .state('SERVICECOMPLETION', {
            url: "/SERVICECOMPLETION",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/HRReport.html?v=24',
                    controller: 'HRReportController'
                }
            },
            params: { 'type': 'SERVICECOMPLETION' },
            data: {
                pageTitle: 'SERVICECOMPLETION'
            }
        })

        .state('PENDINGLR', {
            url: "/PENDINGLR",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PendingLR.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'PENDINGLR'
            }
        })

        .state('PRODUCTWISESTOCK', {
            url: "/PRODUCTWISESTOCK",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/ProductWiseStock.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'PRODUCTWISESTOCK'
            }
        })

        .state('STOCKMARKERREP', {
            url: "/STOCKMARKERREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockMarker.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'STOCKMARKERREP' },

            data: {
                pageTitle: 'STOCKMARKERREP',
                permission: 'RETAILSTOCKMARKERREPORT'
            }
        })

        .state('PCREP', {
            url: "/PCREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PriceChange.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'PCREP' },

            data: {
                pageTitle: 'PCREP',
                permission: 'RETAILPRICECHANGERREPORT'
            }
        })

        .state('STOCKAUDITREP', {
            url: "/STOCKAUDITREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockAudit.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'STOCKAUDITREP' },

            data: {
                pageTitle: 'STOCKAUDITREP',
                permission: 'RETAILSTOCKAUDITREPORT'
            }
        })
        .state('WHSTOCKAUDITREP', {
            url: "/WHSTOCKAUDITREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/StockAudit.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'WHSTOCKAUDITREP' },

            data: {
                pageTitle: 'WHSTOCKAUDITREP',
                permission: 'WHSTOCKAUDITREPORT'
            }
        })

        .state('LRENTRYDATE', {
            url: "/LRENTRYDATE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/LREntryDatewise.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'LRENTRYDATE'
            }

        })

        .state('SUPPURCHASERETURN', {
            url: "/SUPPURCHASERETURN",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseReturn.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'SUPPURCHASERETURN'
            }

        })

        .state('INVENTRY', {
            url: "/INVENTRY",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InvoiceEntry.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'INVENTRY'
            }

        })

        .state('LRWISEINVREPORT', {
            url: "/LRWISEINVREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/LrEntryWiseInvoice.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'LRWISEINVREPORT'
            }

        })

        .state('PRODAGAINSTSIZE', {
            url: "/PRODAGAINSTSIZE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/ProductAgainstSize.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'PRODAGAINSTSIZE'
            }

        })

        .state('BRANDAGAINSTSIZE', {
            url: "/BRANDAGAINSTSIZE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BrandAgainstSize.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'BRANDAGAINSTSIZE'
            }

        })

        .state('BARCODEREP', {
            url: "/BARCODEREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/BarcodeReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'BARCODEREP' },

            data: {
                pageTitle: 'BARCODEREP',
                permission: 'SALESREPORT'
            }

        })

        .state('SALEAGEWISE', {
            url: "/SALEAGEWISE",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SalesAgewise.html?v=24',
                    controller: 'RetReportController'
                }
            },
            params: { 'type': 'SALEAGEWISE' },

            data: {
                pageTitle: 'SALEAGEWISE',
                permission: 'SALESREPORT'
            }

        })

        .state('SUPPOUTWARD', {
            url: "/SUPPOUTWARD",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/SupplierWiseOutward.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'SUPPOUTWARD'
            }

        })

        .state('INWARDPENDING', {
            url: "/INWARDPENDING",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InwardPending.html?v=24',
                    controller: 'WHReportController'
                }
            },
            params: { 'type': 'INWARDPENDING' },
            data: {
                pageTitle: 'INWARDPENDING',
                permission: 'INWARDPENDINGREPORT'
            }

        })

        .state('DETAILEDOUTWARD', {
            url: "/DETAILEDOUTWARD",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/DetailedOutward.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'DETAILEDOUTWARD',
                permission: 'OUTWARDREPORT'
            }

        })

        .state('OUTWARDPENDING', {
            url: "/OUTWARDPENDING",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AttributeWisePending.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'OUTWARDPENDING',
                permission: 'OUTWARDPENDINGREPORT'
            }

        })
        .state('BugTracker', {
            url: "/BugTracker",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/BugTracker.html?v=24',
                    controller: 'BugTrackerController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('LeadManagement', {
            url: "/LeadManagement",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LeadManagement.html?v=24',
                    controller: 'LeadManagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LEADMANAGEMENT'
            }
        })
        .state('ATTRIBUTEWISEPENDING', {
            url: "/ATTRIBUTEWISEPENDING",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/AttributeWisePending.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'ATTRIBUTEWISEPENDING'
            }

        })
        .state('SReports', {
            url: "/SReports",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SReports.html?v=24',
                    controller: 'RetReportController'
                }
            },
            data: {
                pageTitle: 'SReports',
                permission: 'RPTRETAIL'
            }

        })

        .state('expenses', {
            url: "/expenses",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Expenses.html?v=24',
                    controller: 'ExpensesController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('PrinterConfig', {
            url: "/PrinterConfig",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PrinterConfig.html?v=24',
                    controller: 'CompanyController'
                }
            },
            params: { 'type': 'PrinterConfig' },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('ledgerreceivable', {
            url: "/ledgerreceivable",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LedgerReceivable.html?v=24',
                    controller: 'LedgerReceivableController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LEDGERRECEIVABLE'
            }
        })

        .state('ledgerpayable', {
            url: "/ledgerpayable",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LedgerPayable.html?v=24',
                    controller: 'LedgerPayableController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LEDGERPAYABLE'
            }
        })

        .state('daybook', {
            url: "/daybook",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DayBook.html?v=24',
                    controller: 'DayBookController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DAYBOOK'
            }
        })

        .state('trailbalance', {
            url: "/trailbalance",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/trailbalance.html?v=24',
                    controller: 'TrailBalanceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TRAILBALANCE'
            }
        })


        .state('dailypl', {
            url: "/dailypl",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/dailypl.html?v=24',
                    controller: 'DailyPLController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DAILYPANDL'
            }
        })


        .state('tradingaccount', {
            url: "/tradingaccount",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/financebalance.html?v=24',
                    controller: 'FinanceBalanceController'
                }
            },
            params: { 'type': 'TRADING' },
            data: {
                pageTitle: 'Lab',
                permission: 'TRADINGACCOUNT'
            }
        })
        .state('profitandloss', {
            url: "/profitandloss",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/financebalance.html?v=24',
                    controller: 'FinanceBalanceController'
                }
            },
            params: { 'type': 'P&L' },
            data: {
                pageTitle: 'Lab',
                permission: 'PROFITANDLOSS'
            }
        })
        .state('balancesheet', {
            url: "/balancesheet",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/financebalance.html?v=24',
                    controller: 'FinanceBalanceController'
                }
            },
            params: { 'type': 'BALANCESHEET' },
            data: {
                pageTitle: 'Lab',
                permission: 'BALANCESHEET'
            }
        })
        .state('petticash', {
            url: "/petticash",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PettiCashManagement.html?v=24',
                    controller: 'PettiCashManagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PETTYCASH'
            }
        })

        .state('DirectPurchase', {
            url: "/DirectPurchase",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DirectPurchaseEntry.html?v=24',
                    controller: 'DirectEntryController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DIRECTPURCHASE'
            }
        })

        .state('ReportBuilder', {
            url: "/ReportBuilder",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ReportBuilder.html?v=24',
                    controller: 'ReportBuilderController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'REPORTBUILDER'
            }
        })

        .state('BankBook', {
            url: "/BankBook",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/BankBook.html?v=24',
                    controller: 'BankBookController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('CRMReports', {
            url: "/CRMReports",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CRMReports.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('CRMAnalyser', {
            url: "/CRMAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CustomerAnalyser.html?v=24',
                    controller: 'CustomerAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERANALYSER'
            }
        })

        .state('CustomerBday', {
            url: "/CustomerBday",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerBirthday.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORT'
            }
        })

        .state('CustomerAnniversary', {
            url: "/CustomerAnniversary",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerAnniversary.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORT'
            }
        })

        .state('InactiveCustomer', {
            url: "/InactiveCustomer",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/InactiveCustomer.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORT'
            }
        })

        .state('activeCustomer', {
            url: "/activeCustomer",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerCapacity.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORT'
            }
        })
        .state('ScheduleLog', {
            url: "/ScheduleLog",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/ScheduleMessageLog.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SCHEDULAR'
            }
        })

        .state('BillPrint', {
            url: "/BillPrint",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/BillPrint.html?v=24',
                    controller: 'BillPrintController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BILLPRINT'
            }
        })

        .state('LoyaltyConfiguration', {
            url: "/LoyaltyConfiguration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LoyaltyConfiguration.html?v=24',
                    controller: 'LoyaltyConfigurationController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('LoyaltyMgmt', {
            url: "/LoyaltyMgmt",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LoyaltyManagement.html?v=24',
                    controller: 'LoyaltyManagementController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERLOYALTYMGMT'
            }
        })

        .state('PRPending', {
            url: "/PRPending",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PRPendingReport.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PENDINGPRF'
            }
        })

        .state('PRCancelled', {
            url: "/PRCancelled",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PRCancelledReport.html?v=24',
                    controller: 'WHReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CANCELLEDPRF'
            }
        })

        .state('CustomerAdvManager', {
            url: "/CustomerAdvManager",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/CustomerAdvanceManagement.html?v=24',
                    controller: 'CustomerOrderontroller'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERORDERS'
            }
        })

        .state('SystemMapper', {
            url: "/SystemMapper",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SystemMapper.html?v=24',
                    controller: 'SystemMapperController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SYSTEMMAPPER'
            }
        })

        .state('loanreconciliation', {
            url: "/loanreconciliation",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/LoanReconciliation.html?v=24',
                    controller: 'LoanReconciliationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LOANRECONCILIATION'
            }
        })

        .state('POREPORT', {
            url: "/POREPORT",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PurchaseOrderReport.html?v=24',
                    controller: 'PurchaseOrderController'
                }
            },
            data: {
                pageTitle: 'POREPORT',
                permission: 'PURCHASEORDERREPORT'
            }

        })

        .state('PODetailsReport', {
            url: "/PODetailsReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PODetailReport.html?v=24',
                    controller: 'PurchaseOrderController'
                }
            },
            data: {
                pageTitle: 'PODetailsReport',
                permission: 'PURCHASEORDERREPORT'
            }

        })

        .state('PendingPO', {
            url: "/PendingPO",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/PendingOrders.html?v=24',
                    controller: 'PurchaseOrderController'
                }
            },
            data: {
                pageTitle: 'PendingPO',
                permission: 'PURCHASEORDERREPORT'
            }

        })

        .state('CustomerActivity', {
            url: "/CustomerActivity",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerActivity.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'CustomerActivity',
                permission: 'CUSTOMERREPORT'
            }

        })

        .state('TopCustomers', {
            url: "/TopCustomers",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/TopCustomers.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'TopCustomers'
            }

        })

        .state('finanalyser', {
            url: "/finanalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/finanalyser.html?v=24',
                    controller: 'FinanalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FINANALYSER'
            }
        })

        .state('fintds', {
            url: "/fintds",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/fintds.html?v=24',
                    controller: 'FintdsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FINTDS'
            }
        })

        .state('fingst', {
            url: "/fingst",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/fingst.html?v=24',
                    controller: 'FingstController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FINGST'
            }
        })

        .state('BRS', {
            url: "/BRS",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/BRS.html?v=24',
                    controller: 'BRSController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BRS'
            }
        })

        .state('ChequeBox', {
            url: "/ChequeBox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ChequeBox.html?v=24',
                    controller: 'ChequeBoxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CHEQUEBOOK'
            }
        })

        .state('transportpayment', {
            url: "/transportpayment",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TransportPayment.html?v=24',
                    controller: 'TransportPaymentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TRANSPORTPAYMENT'
            }
        })
        .state('agentpayment', {
            url: "/agentpayment",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/agentpayment.html?v=24',
                    controller: 'AgentPaymentController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'AGENTPAYMENT'
            }
        })
        .state('incentiveprocess', {
            url: "/incentiveprocess",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/IncentiveProcess.html?v=24',
                    controller: 'IncentiveProcessController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'INCENTIVEREPORT'
            }
        })

        .state('datamigration', {
            url: "/datamigration",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DataMigration.html?v=24',
                    controller: 'DataMigrationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DATAMIGRATION'
            }
        })

        .state('UserActivity', {
            url: "/UserActivity",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/UserActivity.html?v=24',
                    controller: 'UserActivityController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('Schedular', {
            url: "/Schedular",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Schedular.html?v=24',
                    controller: 'SchedularController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SCHEDULAR'
            }
        })

        .state('SendSMS', {
            url: "/SendSMS",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/SendSMS.html?v=24',
                    controller: 'SendSMSController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORTS'
            }
        })
        .state('DOA', {
            url: "/DOA",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/DeathOnArrival.html?v=24',
                    controller: 'DOAController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DEATHONARRIVAL'
            }
        })

        .state('PriceDrop', {
            url: "/PriceDrop",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/PriceDrop.html?v=24',
                    controller: 'PriceDropController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'PRICEDROP'
            }
        })

        .state('DOAIssue', {
            url: "/DOAIssue",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/DOAIssue.html?v=24',
                    controller: 'DOAIssueController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })


        .state('Recycle', {
            url: "/Recycle",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/Recycle.html?v=24',
                    controller: 'RecycleController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RECYCLE'
            }
        })

        .state('StoreTransaction', {
            url: "/StoreTransaction",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/StoreTransaction.html?v=24',
                    controller: 'StoreTransactionController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STORETRANSACTION'
            }
        })

        .state('DailyPrice', {
            url: "/DailyPrice",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DailyPrice.html?v=24',
                    controller: 'DailyPriceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'DAILYPRICE'
            }
        })

        .state('FineCalc', {
            url: "/FineCalc",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/FineCalculation.html?v=24',
                    controller: 'FineCalculationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FINEREPORT'
            }
        })
        .state('BuyBack', {
            url: "/BuyBack",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/BuyBack.html?v=24',
                    controller: 'BuyBackController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BUYBACK'
            }
        })
        .state('BuyBackScheme', {
            url: "/BuyBackScheme",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/BuyBackScheme.html?v=24',
                    controller: 'BuyBackSchemeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BUYBACKSCHEME'
            }
        })
        .state('EMIScheme', {
            url: "/EMIScheme",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/EMIScheme.html?v=24',
                    controller: 'EMISchemeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMISCHEME'
            }
        })
        .state('Recharge', {
            url: "/Recharge",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/Recharge.html?v=24',
                    controller: 'RechargeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RECHARGE'
            }
        })
        .state('RechargeScheme', {
            url: "/RechargeScheme",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mobile/views/RechargeScheme.html?v=24',
                    controller: 'RechargeSchemeController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RECHARGESCHEME'
            }
        })
        //.state('MobileDailyPrice', {
        //    url: "/MobileDailyPrice",
        //    views: {
        //        'container@': {
        //            templateUrl: 'clientside/Mobile/views/DailyPrice.html?v=24',
        //            controller: 'MobileDailyPriceController'
        //        }
        //    },
        //    data: {
        //        pageTitle: 'Lab',
        //        permission: 'DAILYPRICE'
        //    }
        //})
        .state('PaymentSchedular', {
            url: "/PaymentSchedular",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PaymentScheduler.html?v=24',
                    controller: 'PaymentSchedulerController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('POSInvoice', {
            url: "/POSInvoice",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/POSInvoice.html?v=24',
                    controller: 'POSInvoiceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'POSINVOICE'
            }
        })
        .state('POSInvoiceReturn', {
            url: "/POSInvoiceReturn",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/POSInvoice.html?v=24',
                    controller: 'POSInvoiceController'
                }
            },
            params: { 'type': 'SALERETURN' },
            data: {
                pageTitle: 'Invoice Sales Return',
                permission: 'POSINVOICE'
            }
        })
        .state('DeliveryChallan', {
            url: "/DeliveryChallan",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DeliveryChallan.html?v=24',
                    controller: 'DeliveryChallanController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('DCtoPO', {
            url: "/DCtoPO",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/DCtoPO.html?v=24',
                    controller: 'DCtoPOController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('Notifications', {
            url: "/Notifications",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/Notification.html?v=24',
                    controller: 'NotificationsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('PermissionSlip', {
            url: "/PermissionSlip",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/PermissionSlip.html?v=24',
                    controller: 'PermissionSlipController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'EMPLOYEEPERMISSION'
            }
        })

        .state('OpeningBalance', {
            url: "/OpeningBalance",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/OpeningBalance.html?v=24',
                    controller: 'OpningBalanceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'LEDGEROB'
            }
        })
        .state('Empadvsummary', {
            url: "/Empadvsummary",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/EmployeeAdvancePending.html?v=24',
                    controller: 'RetReportController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('ROLStoreview', {
            url: "/ROLStoreview",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/ROLStoreView.html?v=24',
                    controller: 'ROLStoreViewController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('OPENINGCLOSINGREP', {
            url: "/OPENINGCLOSINGREP",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/OpeningClosingReport.html?v=24',
                    controller: 'RetReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SETTLEMENTOBCLREPORT'
            }
        })

        .state('RewardManager', {
            url: "/RewardManager",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/RewardManagement.html?v=24',
                    controller: 'RewardManagerController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })


        .state('mfgdashboard', {
            url: "/mfgdashboard",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/MfgDashboard.html?v=24',
                    controller: 'MfgDashboardController'
                }
            },
            data: {
                pageTitle: 'Dashboard',
                permission: 'HOMEPAGE'
            }
        })

        .state('MfgAnalyser', {
            url: "/MfgAnalyser",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/StockAnalyser.html?v=24',
                    controller: 'MfgAnalyserController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'MFGANALYSER'
            }
        })


        .state('ComponentInward', {
            url: "/ComponentInward",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/Inward.html?v=24',
                    controller: 'ComponentInwardController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('ComponentOutward', {
            url: "/ComponentOutward",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/Outward.html?v=24',
                    controller: 'ComponentOutwardController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('WOOrchestration', {
            url: "/WOOrchestration",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/WOOrchestration.html?v=24',
                    controller: 'WOOrchestrationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'WOORCHESTRATION'
            }
        })

        .state('Mfgswatch', {
            url: "/Mfgswatch",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/Mfgswatch.html?v=24',
                    controller: 'MfgswatchController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SWATCH'
            }
        })

        .state('workorder', {
            url: "/workorder",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/workorder.html?v=24',
                    controller: 'WorkOrderController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'WORKORDER'
            }
        })
        .state('BOMRequest', {
            url: "/BOMRequest",
            views: {
                'container@': {
                    templateUrl: 'clientside/mfg/views/BOMRequest.html?v=24',
                    controller: 'BOMRequestController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOMREQUEST'
            }
        })
        .state('woinbox', {
            url: "/woinbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/WOinbox.html?v=24',
                    controller: 'WOInboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'WOINBOX'
            }
        })
        .state('bominbox', {
            url: "/bominbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/bominbox.html?v=24',
                    controller: 'BOMInboxController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('GatePass', {
            url: "/GatePass",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/GatePass.html?v=24',
                    controller: 'GatePassController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('mfgsampleinbox', {
            url: "/mfgsampleinbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/sampleinbox.html?v=24',
                    controller: 'SampleInboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SAMPLEINBOX'
            }
        })
        .state('mfgsampleorder', {
            url: "/mfgsampleorder",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/sampleorder.html?v=24',
                    controller: 'SampleOrderController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SAMPLERORDER'
            }
        })
        .state('wogrn', {
            url: "/wogrn",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/WOGrn.html?v=24',
                    controller: 'WOGrnController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'WOGRN'
            }
        })

        .state('stockbinbox', {
            url: "/stockbinbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/StockBinbox.html?v=24',
                    controller: 'StockBinboxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BINMANAGER'
            }
        })

        .state('joborder', {
            url: "/joborder",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/joborder.html?v=24',
                    controller: 'JobOrderController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'JOBORDER'
            }
        })

        .state('skucreation', {
            url: "/skucreation",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/SKUCreation.html?v=24',
                    controller: 'SKUCreationController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'SKUMASTER'
            }
        })
        .state('BinStorage', {
            url: "/BinStorage",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BinStorage.html?v=24',
                    controller: 'BinStorageController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BINSTORAGE'
            }
        })


        .state('UniqueSequence', {
            url: "/UniqueSequence",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/UniqueSequence.html?v=24',
                    controller: 'UniqueSequenceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'UNIQUESEQUENCE'
            }
        })


        .state('bommaster', {
            url: "/bommaster",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BOMMaster.html?v=24',
                    controller: 'BOMMasterController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOMMASTER'
            }
        })

        .state('BOMPurchaseReturn', {
            url: "/BOMPurchaseReturn",
            params: {
                'directlaunch': true,
                'data': {}
            },
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/PurchaseReturn.html?v=24',
                    controller: 'BOMPurchaseReturnController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOMINVOICERETURN'
            }
        })

        .state('FGBundle', {
            url: "/FGBundle",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/FGBundle.html?v=24',
                    controller: 'FGBundleController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('BOMOutward', {
            url: "/BOMOutward",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BOMBundles.html?v=24',
                    controller: 'BOMBundlesController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'COMPONENTOUTWARD'
            }
        })
        .state('BOMInward', {
            url: "/BOMInward",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BOMInward.html?v=24',
                    controller: 'BOMInwardsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'COMPONENTINWARD'
            }
        })

        .state('ComponentPO', {
            url: "/ComponentPO",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/ComponentPO.html?v=24',
                    controller: 'ComponentPOController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'COMPONENTPO'
            }
        })


        .state('BOMConsumption', {
            url: "/BOMConsumption",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BOMConsumption.html?v=24',
                    controller: 'BOMConsumptionController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOMCONSUMPTION'
            }
        })

        .state('ADDON_TARGETWITHMARGIN', {
            url: "/ADDON_TARGETWITHMARGIN",
            views: {
                'container@': {
                    templateUrl: 'clientside/addon/TARGETWITHMARGIN.html?v=24',
                    controller: 'AddOnsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TARGETWITHMARGIN'
            }
        })

        .state('MFGReports', {
            url: "/MFGReports",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/MFGReports.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'RPTMFG'
            }
        })

        .state('WOGRNReport', {
            url: "/WOGRNReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/WOGrnReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })


        .state('JOReport', {
            url: "/JOReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/JOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('WOReport', {
            url: "/WOReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/WOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('JOPendingReport', {
            url: "/JOPendingReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/JOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            params: {
                'type': 'PENDING'
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('WOPendingReport', {
            url: "/WOPendingReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/WOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            params: {
                'type': 'PENDING'
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('CompGrnReport', {
            url: "/CompGrnReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/ComponentGRNReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('BOMMasterReport', {
            url: "/BOMMasterReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMMasterReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('BOMConsumptionReport', {
            url: "/BOMConsumptionReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMConsumptionReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })


        .state('BOMStockReport', {
            url: "/BOMStockReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMStockReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('BOMOutwardReport', {
            url: "/BOMOutwardReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMOutwardReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })
        .state('BOMOutwardDetailReport', {
            url: "/BOMOutwardDetailReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMOutwardDetailReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('MfgIncentiveReport', {
            url: "/MfgIncentiveReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgIncentiveReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })


        .state('BOMInwardReport', {
            url: "/BOMInwardReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/BOMInwardReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })


        .state('SKUDetail', {
            url: "/SKUDetail",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/SKUDetailReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('FBDashboard', {
            url: "/FBDashboard",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/FBDashboard.html?v=24',
                    controller: 'FBDashboardController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FBDASHBOARD'
            }
        })

        .state('FBQuestion', {
            url: "/FBQuestion",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/FBQuestion.html?v=24',
                    controller: 'FBQuestionController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FBQUESTION'
            }
        })

        .state('FBReport', {
            url: "/FBReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/FBReport.html?v=24',
                    controller: 'FBReportController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FBREPORT'
            }
        })

        .state('FBConfig', {
            url: "/FBConfig",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/FBConfig.html?v=24',
                    controller: 'FBConfigController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'FBCONFIG'
            }
        })
        .state('CRMConfig', {
            url: "/CRMConfig",
            views: {
                'container@': {
                    templateUrl: 'clientside/Feedback/views/CRMConfig.html?v=24',
                    controller: 'CRMConfigController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CRMCONFIG'
            }
        })
        .state('JobWorkInBox', {
            url: "/JobWorkInBox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/JobWorkInBox.html?v=24',
                    controller: 'JobWorkInBoxController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOREJOBWORK'
            }
        })

        .state('JobWorkSample', {
            url: "/JobWorkSample",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/JobWorkSample.html?v=24',
                    controller: 'JobworkSampleController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'JOBWORKSAMPLE'
            }
        })

        .state('JobWorkGRN', {
            url: "/JobWorkGRN",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/JobWorkGRN.html?v=24',
                    controller: 'JobWorkGRNController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STOREJOBWORKGRN'
            }

        })

        .state('JobWork', {
            url: "/JobWork",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/JobWork.html?v=24',
                    controller: 'JobWorkController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('FeedbackReport', {
            url: "/FeedbackReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Reports/views/CustomerFeedbackReport.html?v=24',
                    controller: 'CRMReportsController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'CUSTOMERREPORT'
            }
        })

        .state('CompontPOReport', {
            url: "/CompontPOReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/CompontPOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })

        .state('CompontPurchaseReport', {
            url: "/CompontPurchaseReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/CompontPurchaseReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }

        })


        .state('SKUStock', {
            url: "/SKUStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/SODesignwiseStock.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgPackingSlipReport', {
            url: "/MfgPackingSlipReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgPackingSlipReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('MfgPSComboReport', {
            url: "/MfgPSComboReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgPSComboReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('UIDStock', {
            url: "/UIDStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/UIDStock.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('WorkorderSampleStatus', {
            url: "/WorkorderSampleStatus",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgWorkorderSampleStatus.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANStockReport', {
            url: "/MfgUANStockReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANStockReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANStockTransactionReport', {
            url: "/MfgUANStockTransactionReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANStockTransactionReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANInvoiceReport', {
            url: "/MfgUANInvoiceReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANInvoiceReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANBundleReport', {
            url: "/MfgUANBundleReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANBundleReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANPOReport', {
            url: "/MfgUANPOReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANPOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANSOReport', {
            url: "/MfgUANSOReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANSOReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('MfgUANSalesReport', {
            url: "/MfgUANSalesReport",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/MfgUANSalesReport.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })
        .state('SODesignPlanner', {
            url: "/SODesignPlanner",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/SODesignPlanner.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('SKUDesignStock', {
            url: "/SKUDesignStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/StockDesignwiseStock.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('SKULocationStock', {
            url: "/SKULocationStock",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/SOLocationwiseStock.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('TailoringOrder', {
            url: "/TailoringOrder",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TailoringOrder.html?v=24',
                    controller: 'TailoringOrderController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'STORETAILORINGORDER'
            }
        })

        .state('TailoringInward', {
            url: "/TailoringInward",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TailoringOrderInward.html?v=24',
                    controller: 'TailoringInwardController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('TailoringOutward', {
            url: "/TailoringOutward",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/TailoringOrderOutward.html?v=24',
                    controller: 'TailoringOutwardController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })


        .state('MfgTailoring', {
            url: "/MfgTailoring",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/MfgTailoring.html?v=24',
                    controller: 'MfgTailoringController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'TAILORING'
            }
        })

        .state('SKUSales', {
            url: "/SKUSales",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/SODesignwiseSales.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('TailoringRep', {
            url: "/TailoringRep",
            views: {
                'container@': {
                    templateUrl: 'clientside/Mfg/MfgReports/TailoringDetail.html?v=24',
                    controller: 'MFGReportsController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('gstinbox', {
            url: "/gstinbox",
            views: {
                'container@': {
                    templateUrl: 'clientside/views/GSTInbox.html?v=24',
                    controller: 'GSTInboxController'
                }
            },
            data: {
                pageTitle: 'Lab'
            }
        })

        .state('bominvoice', {
            url: "/bominvoice",
            views: {
                'container@': {
                    templateUrl: 'clientside/MFG/views/BomInvoice.html?v=24',
                    controller: 'BomInvoiceController'
                }
            },
            data: {
                pageTitle: 'Lab',
                permission: 'BOMINVOICE'
            }
        });
    $urlRouterProvider.otherwise('/home');
};


app.directive('onSizeChanged', ['$window', '$rootScope', function ($window, $rootScope) {
    return {
        restrict: 'A',
        scope: {
            onSizeChanged: '&'
        },
        link: function (scope, $element, attr) {
            var element = $element[0];

            cacheElementSize(scope, element);
            $rootScope.windowSize = { 'width': element.offsetWidth, 'height': element.offsetHeight };
            $window.addEventListener('resize', onWindowResize);

            function cacheElementSize(scope, element) {
                scope.cachedElementWidth = element.offsetWidth;
                scope.cachedElementHeight = element.offsetHeight;
            }

            function onWindowResize() {

                var isSizeChanged = scope.cachedElementWidth != element.offsetWidth || scope.cachedElementHeight != element.offsetHeight;
                if (isSizeChanged) {
                    $rootScope.windowSize = { 'height': element.offsetHeight, 'width': element.offsetWidth };
                    cacheElementSize(scope, element);
                    var expression = scope.onSizeChanged();
                    if (typeof (expression) != 'undefined')
                        expression();
                    $rootScope.$broadcast("windowSizeChanged", $rootScope.windowSize);
                }
            };

            onWindowResize();
        }
    }
}]);


app.config(stateProviderConfig);



app.run(['$rootScope', '$state', 'toastr', 'localStorageService', '$http', '$window', 'ngDialog', 'restService', 'ngAuthSettings', '$timeout','$parse',
    function ($rootScope, $state, toastr, localStorageService, $http, $window, ngDialog, restService, ngAuthSettings, $timeout,$parse) {
        $rootScope.$state = $state;
        $rootScope.enterKeyPressed = function (e) {
            if (e.keyCode === 13) {
                //alert('hi');
                //$rootScope.$broadcast("EnterKeyPressed");
            }
        };

        $rootScope.OnUserInfoReady = function (handler,count) {
            if (typeof ($rootScope.UserInfo) != 'undefined' && $rootScope.UserInfo != null) {
                handler($rootScope.UserInfo);
            }
            else {
                if (count == undefined || count == null) 
                    count = 0;

                if (count < 20) {
                    $timeout(function () {
                        $rootScope.OnUserInfoReady(handler, count++);
                    }, 50)
                }
            }
        }

        $rootScope.OnLocationInfoReady = function (handler, count) {
            if (typeof ($rootScope.LocationInfo) != 'undefined' && $rootScope.LocationInfo != null) {
                handler($rootScope.LocationInfo);
            }
            else {
                if (count == undefined || count == null)
                    count = 0;

                if (count < 20) {
                    $timeout(function () {
                        $rootScope.OnLocationInfoReady(handler, count++);
                    }, 50)
                }
            }
        }

        $rootScope.BindLocation = function (scope, key, callback) {
            var timer = 50;
            $timeout(function () {

                $rootScope.OnLocationInfoReady(function () {
                    $parse(key).assign(scope, $rootScope.LocationInfo.location.id);
                    if (callback != undefined && callback != null)
                        callback();
                })

            }, timer)
        }

        $rootScope.BindCompany = function(scope, key, callback)
        {
            var timer = 50;
            $timeout(function () {

                $rootScope.OnLocationInfoReady(function () {
                    $parse(key).assign(scope, $rootScope.LocationInfo.company.id);
                    if (callback != undefined && callback != null)
                        callback();
                })
                
            }, timer)
        }


        $rootScope.$on('$stateChangeStart',
            function (event, toState, fromState) {
                if (typeof (toState.data) != 'undefined' && toState.data != null &&
                    typeof ($rootScope.UserInfo) != 'undefined' && $rootScope.UserInfo != null &&
                    typeof ($rootScope.UserInfo['p_' + toState.data.permission]) != 'undefined' &&
                    $rootScope.UserInfo['p_' + toState.data.permission] != null &&
                    !$rootScope.UserInfo['p_' + toState.data.permission].view)
                    event.preventDefault();

                if ($rootScope.IsKiosk && 
                    toState.name) {
                    if (toState.name != 'NonTextileSale')
                        event.preventDefault();
                }

                if (typeof (toState.data) != 'undefined' && toState.data != null &&
                    typeof ($rootScope.UserInfo) != 'undefined' &&
                    $rootScope.License != null &&
                    $rootScope.License[toState.data.license] &&
                    $rootScope.License[toState.data.license] != null &&
                    !$rootScope.License[toState.data.license])
                    event.preventDefault();

                if ($rootScope.ShowLicenseMessage) {
                    $rootScope.LicenseMessage();
                }

            });

        function Toaster(toastr) {
            this.toastr = toastr;
            this.Debug = function (msg) {
                toastr.info(msg, 'Information', { timeOut: 10000, allowHtml: true });
            };
            this.Info = function (msg) {
                toastr.info(msg, 'Information', { timeOut: 1500, allowHtml: true });
            };
            this.Notice = function (msg) {
                toastr.info(msg, 'Information', { timeOut: 500, allowHtml: true });
            };
            this.Error = function (msg) {
                toastr.error(msg, 'Error', { timeOut: 5000, allowHtml: true });
            };
        };

        function TopNavigation(supportprint, supportdelete) {
            this.create = true;
            this.save = true;
            this.search = true;
            this.delete = typeof (supportdelete) == 'undefined' ? false : supportdelete;
            this.print = typeof (supportprint) == 'undefined' ? false : supportprint;
            this.edit = false;
            this.searchentity = false;
            this.supportprint = this.print;
            this.supportdelete = this.delete;
            this.SetMode = function (mode) {
                if (mode == 'SEARCH') {
                    this.create = false;
                    this.search = false;
                    this.save = false;
                    if (this.supportprint)
                        this.print = false;
                    this.edit = true;
                    this.searchentity = true;
                } else if (mode == "EDIT") {
                    this.create = true;
                    this.search = true;
                    this.save = true;
                    if (this.supportprint)
                        this.print = true;
                    this.edit = false;
                    this.searchentity = false;
                }
            }
        }

        $rootScope.CheckF2 = function (event, focusid) {
            if (event.key == "F2")
                $scope['setFocus' + focusid]();
        }

        $rootScope.GetNavigation = function (supportprint, supportdelete) {
            return new TopNavigation(supportprint, supportdelete);
        };

        /*        $scope.RoundingMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Rounding-5' } , { 'id': 2, 'name': 'Down-5' },
                { 'id': 3, 'name': 'Up-5' }, { 'id': 4, 'name': 'Down-10' }, { 'id': 5, 'name': 'Up-10' }, { 'id': 6, 'name': 'Rounding-10' } ];
    */

        $rootScope.RoundRupee = function (value, mode) {
            switch (mode) {
                case 1:
                    var remin = value % 5;
                    return value + (remin > 2.5 ? (5 - remin) : (remin * -1));
                    break;
                case 2:
                    var remin = value % 5;
                    return value - remin;
                    break;
                case 3:
                    var remin = value % 5;
                    return value + (5 - remin);
                    break;
                case 4:
                    var remin = value % 10;
                    return value - remin;
                    break;
                case 5:
                    var remin = value % 10;
                    return value + (10 - remin);
                    break;
                case 6:
                    var remin = value % 10;
                    return value + (remin > 5 ? (10 - remin) : (remin * -1));
                    break;
                case 7:
                    var remin = value % 1;
                    return value - remin;
                    break;
                case 8:
                    var remin = value % 1;
                    return value + (1 - remin);
                    break;
            }
            return value;
        };

        $rootScope.PrintPDF = function (url, filename, type, callback) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            var config = { responseType: 'blob' };
            $http.get(serviceBase + url, config).then(function (response) {
                var blob = response.data;
                if (filename.indexOf('.') < 0) {
                    if (blob.type == 'application/zip')
                        filename += '.zip';
                    if (blob.type == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
                        filename += '.xlsx';
                }
                if ($rootScope.PrintDocument && $rootScope.PrintDocument != null) {
                    window.URL.revokeObjectURL($rootScope.PrintDocument.href);
                    $rootScope.PrintDocument.iframe.remove();
                }
                var parentdocument = document;
                var objFra = parentdocument.createElement('iframe'); 
                $rootScope.PrintDocument =
                {
                    document: document,
                    iframe: objFra,
                    href: window.URL.createObjectURL(blob)
                };
                objFra.id = 'printdialog';
                objFra.style.visibility = "hidden"; 
                objFra.src = $rootScope.PrintDocument.href; 
                objFra.onload = function () {
                    objFra.contentWindow.focus(); 
                    objFra.contentWindow.print(); 
                };
                parentdocument.body.appendChild(objFra);
            });
        }

        $rootScope.Download = function (url, filename, type, callback) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            var config = { responseType: 'blob' };
            $http.get(serviceBase + url, config).then(function (response) {
                var blob = response.data;
                if (filename.indexOf('.') < 0) {
                    if (blob.type == 'application/zip')
                        filename += '.zip';
                    if (blob.type == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
                        filename += '.xlsx';
                }
                if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                    window.navigator.msSaveOrOpenBlob(blob, filename);
                    if (callback && callback != null)
                        callback();
                }
                else {
                    var e = document.createEvent('MouseEvents'),
                        a = document.createElement('a');
                    a.download = filename;
                    a.href = window.URL.createObjectURL(blob);
                    a.dataset.downloadurl = filename;
                    e.initEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                    a.dispatchEvent(e);
                    window.URL.revokeObjectURL(a.href);
                    if (callback && callback != null)
                        callback();
                }
            });
        }

        $rootScope.PostDownload = function (url, request, filename, type) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            var config = { responseType: 'blob' };
            $http.post(serviceBase + url, request, config).then(function (response) {
                var blob = response.data;
                if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                    window.navigator.msSaveOrOpenBlob(blob, filename);
                }
                else {
                    var e = document.createEvent('MouseEvents'),
                        a = document.createElement('a');
                    a.download = filename;
                    a.href = window.URL.createObjectURL(blob);
                    a.dataset.downloadurl = filename;
                    e.initEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
                    a.dispatchEvent(e);
                    window.URL.revokeObjectURL(a.href);
                }
            });
        }

        $rootScope.Toaster = new Toaster(toastr);
        $rootScope.WebPrint = new WebPrintService('Printer');
        $rootScope.ExternalWebPrint = null;
        $rootScope.ConditionalExecute = function (condition, trueconidtion, falsecondition) {
            if (condition)
                return trueconidtion();
            else
                return falsecondition();
        }

        $rootScope.CropImage = function (sourcedata, type, callbackorparent, field, cropwidth) {
            var dialog = ngDialog.open({
                template: 'CrapImage',
                data: [sourcedata, type, cropwidth],
                width: '660px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {

                    $scope.cropper = null;
                    $scope.Capture = function () {
                        var canvas = $scope.cropper.getCroppedCanvas();
                        var imgData = canvas.toDataURL();
                        var request = {
                            "Item": {
                                'image': imgData,
                                'type': $scope.ngDialogData[0]
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "File/StoreImage",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    }


                    $timeout(function () {
                        var canvas = document.getElementById("crapcanvas");
                        var image = new Image();
                        image.onload = function () {
                            var ctx = canvas.getContext("2d");
                            ctx.drawImage(image, 0, 0);
                            if ($scope.ngDialogData[2] == null) {
                                $scope.cropper = new Cropper(canvas);
                            }
                            else {
                                $scope.cropper = new Cropper(canvas, {
                                    viewMode: 3,
                                    zoomable: false,

                                    data: {
                                        width: $scope.ngDialogData[2].width,
                                        height: $scope.ngDialogData[2].height,
                                    },

                                    crop: function (event) {
                                        var width = event.detail.width;
                                        var height = event.detail.height;

                                        if (
                                            width < $scope.ngDialogData[2].width
                                            || height < $scope.ngDialogData[2].height
                                            || width > $scope.ngDialogData[2].width
                                            || height > $scope.ngDialogData[2].height
                                        ) {
                                            $scope.cropper.setData({
                                                width: Math.max($scope.ngDialogData[2].width, Math.min($scope.ngDialogData[2].width, width)),
                                                height: Math.max($scope.ngDialogData[2].height, Math.min($scope.ngDialogData[2].height, height)),
                                            });
                                        }

                                        data.textContent = JSON.stringify($scope.cropper.getData(true));
                                    },
                                });
                            }

                        };
                        image.src = 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + $scope.ngDialogData[0];
                    }, 50);
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value != '') {
                        if (callbackorparent && field) {
                            callbackorparent[field] = data.value;
                        }
                        else {
                            callbackorparent(data.value);
                        }
                    }
                    else {
                        if (callbackorparent && field) {
                            callbackorparent[field] = null;
                        }
                        else {
                            callbackorparent(null);
                        }
                    }
                }
            });
        }

        $rootScope.CaptureImage = function (type, callbackorparent, field, crap, cropwidth) {

            if (typeof (callbackorparent) == 'undefined') {
                this.Toaster.Error('Please create, then click capture');
                return;
            }
            var dialog = ngDialog.open({
                template: 'CaptureImage',
                data: [type, crap, cropwidth],
                width: '660px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.ShowSnap = false;
                    $scope.cropper = null;
                    $scope.Capture = function (closewindow) {
                        var canvas = document.getElementById('canvas');
                        var context = canvas.getContext('2d');
                        var video = document.getElementById('video');
                        context.drawImage(video, 0, 0, 640, 480);
                        $scope.ShowSnap = true;
                        if ($scope.ngDialogData[1] && $scope.cropper == null) {

                            if ($scope.ngDialogData[2] == null) {
                                $scope.cropper = new Cropper(canvas);
                            }
                            else {
                                $scope.cropper = new Cropper(canvas, {
                                    viewMode: 3,
                                    zoomable: false,

                                    data: {
                                        width: $scope.ngDialogData[2].width,
                                        height: $scope.ngDialogData[2].height,
                                    },

                                    crop: function (event) {
                                        var width = event.detail.width;
                                        var height = event.detail.height;

                                        if (
                                            width < $scope.ngDialogData[2].width
                                            || height < $scope.ngDialogData[2].height
                                            || width > $scope.ngDialogData[2].width
                                            || height > $scope.ngDialogData[2].height
                                        ) {
                                            $scope.cropper.setData({
                                                width: Math.max($scope.ngDialogData[2].width, Math.min($scope.ngDialogData[2].width, width)),
                                                height: Math.max($scope.ngDialogData[2].height, Math.min($scope.ngDialogData[2].height, height)),
                                            });
                                        }
                                    },
                                });
                            }

                            return;
                        }
                        else {
                            if (closewindow) {
                                $scope.Save();
                            }
                        }
                    }

                    $scope.Save = function () {
                        var canvas = document.getElementById('canvas');
                        if ($scope.cropper != null)
                            canvas = $scope.cropper.getCroppedCanvas();

                        var imgData = canvas.toDataURL();
                        var request = {
                            "Item": {
                                'image': imgData,
                                'type': $scope.ngDialogData[0]
                            },
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "File/StoreImage",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    }

                    $timeout(function () {

                        var video = document.getElementById('video');

                        // Get access to the camera!
                        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                            // Not adding `{ audio: true }` since we only want video now
                            navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                                //video.src = window.URL.createObjectURL(stream);
                                video.srcObject = stream;
                                video.play();
                            });
                        } else if (navigator.getUserMedia) { // Standard
                            navigator.getUserMedia({ video: true }, function (stream) {
                                video.src = stream;
                                video.play();
                            }, errBack);
                        } else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
                            navigator.webkitGetUserMedia({ video: true }, function (stream) {
                                video.src = window.webkitURL.createObjectURL(stream);
                                video.play();
                            }, errBack);
                        } else if (navigator.mozGetUserMedia) { // Mozilla-prefixed
                            navigator.mozGetUserMedia({ video: true }, function (stream) {
                                video.srcObject = stream;
                                video.play();
                            }, errBack);
                        }

                        //if (typeof ($scope.ngDialogData[0].result.billitems) != 'undefined' &&
                        //    $scope.ngDialogData[0].result.billitems.length > 0) {

                        //    var billitem = $scope.ngDialogData[0].result.billitems.find(function (item) {
                        //        return item.qty < 0;
                        //    });

                        //    if (typeof (billitem) != 'undefined' && billitem != null) {
                        //        $scope.Billitemid = billitem.id;
                        //    }
                        //}
                        $scope.setFocuscapture();
                    }, 100);


                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value != '') {
                        if (callbackorparent && field && field != null) {
                            callbackorparent[field] = data.value;
                        }
                        else {
                            callbackorparent(data.value);
                        }
                    }
                    else {
                        if (callbackorparent && field && field != null) {
                            callbackorparent[field] = null;
                        }
                        else {
                            callbackorparent(null);
                        }
                    }
                }
            });
        }


        $rootScope.Handlers = {};
        $rootScope.stateProviderConfig = stateProviderConfig;
        $rootScope.ClearHidenColumns = function () {
            $('table').each(function (index, tbl) {
                if (typeof ($(tbl).attr('hcol')) != 'undefined') {
                    tbl.resetcolumns();
                };
            });
        }

        $rootScope.ShowAlert = function (title, message) {
            $.alert({
                title: title,
                content: message,
            });
        }

        $rootScope.SendWebData = function (printerData,request,callback) {
            if (typeof (QuantoNativeInterface) != 'undefined' && typeof (printerData.isandroid) != 'undefined') {
                request = JSON.parse(request.data);
                QuantoNativeInterface.SendWebData(request.destination, request.content, function (response) {
                    callback(response)
                });
            }
            else {
                $http({
                    method: request.method,
                    url: request.url,
                    data: request.data
                }).success(function (result) {
                    callback(result)
                });
            }
        }

        $rootScope.Sent2Printer = function (printdocument, printerData, mode, encoded) {
            if (typeof (printerData) == 'undefined' || printerData == null)
                printerData = localStorageService.get('PrinterData');
            if (printerData == null) {
                $rootScope.Toaster.Error('Please select the printer');
                return;
            }

            if (typeof (QuantoNativeInterface) != 'undefined' && typeof (printerData.isandroid) != 'undefined') {
                QuantoNativeInterface.printContent(printdocument, printerData.printertype, printerData.printerid);
            }
            else {

                var request = {
                    'id': printerData.printerid,
                    'content': printdocument,
                    'encoded': encoded && encoded != null ? encoded:'BASE64'
                };

                if (printerData.IsWebPrint) {
                    var printmode = (typeof (mode) != 'undefined' ? mode : '');
                    if (printmode == 'RAW')
                        $rootScope.WebPrint.PrintRAW(request, function () { });
                    else if (printmode == 'PDF')
                        $rootScope.WebPrint.PrintPDF(request, function () { });
                    else
                        $rootScope.WebPrint.Print(request, function () { });
                }
                else {
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: printerData.service + (typeof (mode) != 'undefined' ? mode : ''),
                        data: srequest
                    }).success(function (result) {
                    });
                }
            }
        }

        function printformator() {
            this.Escape = String.fromCharCode(27) + String.fromCharCode(33);

            this.Barcode = function (text, center) {
                if (typeof (center) == 'undefined') center = false;
                //var format = "29,72,1,29,102,0,29,104,81,29,119,2,29,107,73";
                var result = !center ? '' : (String.fromCharCode(27) + 'a' + String.fromCharCode(1));

                result += '\u001dH\u0000\u001df\0\u001dhQ\u001dw\u0002\u001dkI';

                //$.each(format.split(','), function (k, v) {
                //    result += String.fromCharCode(parseInt(v));
                //});
                var nostart = text.search(/[0-9]/);
                if (text.search(/[0-9]/) >= 0) {
                    var leftpart = text.substr(0, nostart);
                    var rightpart = text.substr(nostart);
                    result += String.fromCharCode(text.length + 4);
                    result += "{B" + leftpart + "{A" + rightpart;
                }
                else {
                    result += String.fromCharCode(text.length + 2);
                    result += "{B" + text;
                }

                if (center) {
                    result += String.fromCharCode(10) + String.fromCharCode(27);
                    result += 'a' + String.fromCharCode(0);
                }
                return result;
            }

            this.Large = function (content, length, leftalighn) {
                var val = this.Escape + String.fromCharCode(4);
                if (typeof (length) != 'undefined') {
                    if (typeof (leftalighn) != 'undefined' && leftalighn) {
                        val += content.toString().padEnd(length, ' ');
                    }
                    else {
                        val += content.toString().padStart(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }

            this.Condensed = function (content, length, leftalighn) {
                var val = this.Escape + String.fromCharCode(1);
                if (typeof (length) != 'undefined') {
                    if (typeof (leftalighn) != 'undefined' && leftalighn) {
                        val += content.toString().padEnd(length, ' ');
                    }
                    else {
                        val += content.toString().padStart(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }
            this.Normal = function (content, length, leftalighn) {
                var val = this.Escape + String.fromCharCode(8);
                if (typeof (length) != 'undefined') {
                    if (typeof (leftalighn) != 'undefined' && leftalighn) {
                        val += content.toString().padEnd(length, ' ');
                    }
                    else {
                        val += content.toString().padStart(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }


            this.Bold = function (content, length, leftalighn) {
                var val = this.Escape + String.fromCharCode(47);
                if (typeof (length) != 'undefined') {
                    if (typeof (leftalighn) != 'undefined' && leftalighn) {
                        val += content.toString().padEnd(length, ' ');
                    }
                    else {
                        val += content.toString().padStart(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }
            this.Align = function (content, length, rightalign) {
                var val = '';
                if (typeof (length) != 'undefined') {
                    if (typeof (rightalign) != 'undefined' && rightalign) {
                        val += content.toString().padStart(length, ' ');
                    }
                    else {
                        val += content.toString().padEnd(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }

            this.Underline = function (content, length, leftalighn) {
                var val = this.Escape + String.fromCharCode(141);
                if (typeof (length) != 'undefined') {
                    if (typeof (leftalighn) != 'undefined' && leftalighn) {
                        val += content.toString().padEnd(length, ' ');
                    }
                    else {
                        val += content.toString().padStart(length, ' ');
                    }
                }
                else {
                    val += content;
                }
                return val;
            }
            this.Cutter = function () {
                return String.fromCharCode(29) + "V" + String.fromCharCode(66) + String.fromCharCode(3);
            }
        }

        $rootScope.PrintFormater = new printformator();

        $rootScope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
        }

        $rootScope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            var nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $rootScope.Confirm = function (title, message, onconfirm) {
            $.confirm({
                title: title,
                content: message,
                buttons: {
                    Ok: function () {
                        onconfirm();
                    },
                    Cancel: function () {

                    }
                }
            });
        }


        $rootScope.Prompt = function (title, message, onyes, defaultValue, rows, oncancel) {
            if (event) {
                event.stopPropagation();
                event.preventDefault();
            }
            $.confirm({
                title: title,
                content: '' +
                    '<form action="" class="formName">' +
                    '<div class="form-group">' +
                    '<label>' + message + '</label>' +
                    (typeof (rows) != 'undefined' ? '<textarea class="name form-control input-sm" rows"' + rows + '" ' + ' required >' + (typeof (defaultValue) != 'undefined' ? defaultValue : '') + '</textarea>' :
                        '<input type="text" placeholder="Value" class="name form-control input-sm" ' + (typeof (defaultValue) != 'undefined' ? 'value ="' + defaultValue + '"' : '') + ' required />') +
                    '</div>' +
                    '</form>',
                buttons: {
                    formSubmit: {
                        text: 'Ok',
                        btnClass: 'btn-blue',
                        keys: ['enter'],
                        action: function () {
                            if (event.type == 'keyup') return false;
                            var value = this.$content.find('.name').val();
                            if (!value) {
                                $.alert('provide a valid value');
                                return false;
                            }
                            if (event) {
                                event.cancelBubble = true;
                                event.stopPropagation();
                                event.preventDefault();
                            }
                            return onyes(value,event);
                        }
                    },
                    cancel: function () {
                        if (event) {
                            event.cancelBubble = true;
                            event.stopPropagation();
                            event.preventDefault();
                        }
                        if (oncancel)
                            oncancel();
                    },
                },
                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    this.$content.find('.name').focus();
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.cancelBubble = true;
                        e.stopPropagation();
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        };
    }]);

var serviceBase = '';
app.constant('ngAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'InfyPOS',
    secret: 'KareInfinity'
});

app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
    $httpProvider.defaults.useXDomain = true;
    $httpProvider.defaults.withCredentials = true;
    $httpProvider.defaults.timeout = 100 * 60 * 1000; // 10 minutes
});

app.config(['ngDialogProvider', function (ngDialogProvider) {
    ngDialogProvider.setDefaults({
        className: 'ngdialog-theme-default',
        plain: false,
        showClose: false,
        closeByDocument: true,
        closeByEscape: true,
        appendTo: false,
        preCloseCallback: function () {
            console.log('default pre-close callback');
        }
    });
}]);

app.run(['authService', function (authService) {
    //alert('authservice');
    if (!authService.fillAuthData()) {
        if ($(location).attr('href').indexOf('redir=') < 0)
            $(location).attr('href', 'login.html?redir=' + encodeURI($(location).attr('href')));
    }
}]);


if (!String.prototype.padStart) {
    String.prototype.padStart = function padStart(targetLength, padString) {
        targetLength = targetLength >> 0; //truncate if number or convert non-number to 0;
        padString = String((typeof padString !== 'undefined' ? padString : ' '));
        if (this.length > targetLength) {
            return String(this);
        }
        else {
            targetLength = targetLength - this.length;
            if (targetLength > padString.length) {
                padString += padString.repeat(targetLength / padString.length); //append to original to ensure we are longer than needed
            }
            return padString.slice(0, targetLength) + String(this);
        }
    };
}

if (!String.prototype.padEnd) {
    String.prototype.padEnd = function padEnd(targetLength, padString) {
        targetLength = targetLength >> 0; //floor if number or convert non-number to 0;
        padString = String((typeof padString !== 'undefined' ? padString : ' '));
        if (this.length > targetLength) {
            return String(this);
        }
        else {
            targetLength = targetLength - this.length;
            if (targetLength > padString.length) {
                padString += padString.repeat(targetLength / padString.length); //append to original to ensure we are longer than needed
            }
            return String(this) + padString.slice(0, targetLength);
        }
    };
}

Number.prototype.round = function (places) {
    return +(Math.round(this + "e+" + places) + "e-" + places);
}

String.prototype.getDate = function () {
    var datePart = this.substr(0, this.indexOf('T')).split('-');
    return new Date(parseInt(datePart[0]), parseInt(datePart[1]), parseInt(datePart[2]), 0, 0, 0, 0);
}

Date.prototype.datePart = function () {
    //var year = this.getFullYear();
    //var month = this.getMonth();
    //var date = this.getDate();
    //this.local();
    var year = this.getFullYear();
    var month = this.getMonth();
    var date = this.getDate();
    return new Date(year, month, date, 0, 0, 0, 0);
}

Date.prototype.addMonths = function (m) {
    var d = new Date(this);
    var years = Math.floor(m / 12);
    var months = m - (years * 12);
    if (years) d.setFullYear(d.getFullYear() + years);
    if (months) d.setMonth(d.getMonth() + months);
    return d;
}

Date.prototype.addDays = function (days) {
    var dat = new Date(this.valueOf());
    dat.setDate(dat.getDate() + days);
    return dat;
}

app.directive('fileLoad', ['$timeout', 'localStorageService', '$parse', '$compile', '$rootScope', function ($timeout, localStorageService,
    $parse, $compile, $rootScope) {
    return {
        restrict: 'E',
        template: '<div ng-transclude></div>',
        replace: true,
        transclude: true,
        require: 'ngModel',
        link: function (scope, el, attr, ngModel) {
            var fileName,
                shareCredentials,
                withPreview,
                fileSelector,
                resize,
                maxWidth,
                maxHeight,
                sel,
                multiple,
                filemodel,
                primaryinstitutionid,
                filetype,
                onuploadcomplete,
                onlyupload;

            fileName = attr.name || 'userFile';
            shareCredentials = attr.credentials === 'true';
            withPreview = attr.preview === 'true';
            resize = attr.resize === 'true';
            multiple = attr.multiple == 'true'
            onlyupload = attr.onlyupload || 'false' == 'true';
            maxWidth = angular.isDefined(attr.maxWidth) ? parseInt(attr.maxWidth) : false;
            maxHeight = angular.isDefined(attr.maxHeight) ? parseInt(attr.maxHeight) : false;
            fileSelector = angular.isDefined(attr.fileSelector) ? attr.fileSelector : false;
            primaryinstitutionid = angular.isDefined(attr.primaryinstitutionid) ? attr.primaryinstitutionid : scope.UserInfo.OrganizationID;
            filemodel = attr.filemodel;
            filetype = typeof (attr.filetype) == 'undefined' ? 'pdf' : attr.filetype;
            onuploadcomplete = typeof (attr.onuploadcomplete) == 'undefined' ? null : attr.onuploadcomplete;

            var btnstyle = el.hasClass('btn-xs') ? 'btn-xs' : 'btn-sm';
            el.append('<input style="display: none !important;" type="file" ' + (multiple ? 'multiple' : '') + ' accept="' + (attr.accept ? attr.accept : '') + '" name="' + fileName + '"/>');
            var roolelement = el.parent().parent();
            var elementarray = new Array();
            if (!onlyupload) {
                //elementarray.push('<label ng-hide="'+filemodel+'!=null" class="col-sm-7 control-label">{{Application.birthcertificate.fileInfo.name}}</label>');
                elementarray.push('<div class="col-sm-6 pull-right no-padding" ng-show="' + filemodel + '!=null && ' + filemodel + "!=''" + '">');
                elementarray.push('<a ');
                elementarray.push(' class="col-sm-4 btn btn-success btn-rad btn-trans ' + btnstyle + ' file-upload" target="_blank" href="File/Download/{{' + primaryinstitutionid + '}}/{{' + filemodel + '}}">');
                elementarray.push('<i class="fa fa-download"></i>');
                elementarray.push('</a>');
                elementarray.push('<a ');
                elementarray.push(' class="col-sm-4 btn btn-success btn-rad btn-trans ' + btnstyle + ' file-upload" target="_blank" href="File/Show/{{' + primaryinstitutionid + '}}/{{' + filemodel + '}}">');
                elementarray.push('<i class="fa fa-file-image-o"></i>');
                elementarray.push('</a>');
                elementarray.push('<button ');
                elementarray.push(' class="col-sm-4 btn btn-danger ' + btnstyle + ' btn-rad btn-trans file-upload"');
                elementarray.push(' ng-click="RemoveImage(' + filemodel + ')">');
                elementarray.push('<i class="fa fa-trash-o"></i>');
                elementarray.push('</button>');
                elementarray.push('</div>');
            }

            // Step 1: parse HTML into DOM element
            var template = angular.element(elementarray.join(' '));
            // Step 2: compile the template
            var linkFn = $compile(template);
            // Step 3: link the compiled template with the scope.
            var element = linkFn(scope);
            // Step 4: Append to DOM (optional)
            roolelement.append(element);


            $timeout(function () {
                sel = fileSelector ? angular.element(el[0].querySelectorAll(fileSelector)[0]) : el;
                sel.bind('click', function () {
                    if (!scope.disabled) {
                        scope.$eval(el.find('input')[0].click());
                    }
                });
            });

            angular.element(el.find('input')[0]).bind('change', function (e) {
                var files = e.target.files;

                var fileReader = new FileReader();
                fileReader.onload = function () {
                    scope[onuploadcomplete](fileReader.result, scope);
                }
                fileReader.readAsText(files[0]);
                scope.$apply();
            })
        }
    }
}]);


app.directive('fileUpload', ['$timeout', 'localStorageService', '$parse', '$compile', '$rootScope', function ($timeout, localStorageService,
    $parse, $compile, $rootScope) {
    return {
        restrict: 'E',
        template: '<div ng-transclude></div>',
        replace: true,
        transclude: true,
        require: 'ngModel',
        link: function (scope, el, attr, ngModel) {
            var fileName,
                shareCredentials,
                withPreview,
                fileSelector,
                resize,
                maxWidth,
                maxHeight,
                sel,
                multiple,
                filemodel,
                primaryinstitutionid,
                filetype,
                onuploadcomplete,
                onuploadstart,
                onlyupload;

            fileName = attr.name || 'userFile';
            shareCredentials = attr.credentials === 'true';
            withPreview = attr.preview === 'true';
            resize = attr.resize === 'true';
            multiple = attr.multiple == 'true'
            onlyupload = attr.onlyupload || 'false' == 'true';
            maxWidth = angular.isDefined(attr.maxWidth) ? parseInt(attr.maxWidth) : false;
            maxHeight = angular.isDefined(attr.maxHeight) ? parseInt(attr.maxHeight) : false;
            fileSelector = angular.isDefined(attr.fileSelector) ? attr.fileSelector : false;
            primaryinstitutionid = angular.isDefined(attr.primaryinstitutionid) ? attr.primaryinstitutionid : scope.UserInfo.OrganizationID;
            filemodel = attr.filemodel;
            filetype = typeof (attr.filetype) == 'undefined' ? 'pdf' : attr.filetype;
            onuploadcomplete = typeof (attr.onuploadcomplete) == 'undefined' ? null : attr.onuploadcomplete;
            onuploadstart = typeof (attr.onuploadstart) == 'undefined' ? null : attr.onuploadstart;
            var btnstyle = el.hasClass('btn-xs') ? 'btn-xs' : 'btn-sm';
            el.append('<input style="display: none !important;" type="file" ' + (multiple ? 'multiple' : '') + ' accept="' + (attr.accept ? attr.accept : '') + '" name="' + fileName + '"/>');
            var roolelement = el.parent().parent();
            var elementarray = new Array();
            if (!onlyupload) {
                //elementarray.push('<label ng-hide="'+filemodel+'!=null" class="col-sm-7 control-label">{{Application.birthcertificate.fileInfo.name}}</label>');
                elementarray.push('<div class="col-sm-6 pull-right no-padding" ng-show="' + filemodel + '!=null && ' + filemodel + "!=''" + '">');
                elementarray.push('<a ');
                elementarray.push(' class="col-sm-4 btn btn-success btn-rad btn-trans ' + btnstyle + ' file-upload" target="_blank" href="File/Download/{{' + primaryinstitutionid + '}}/{{' + filemodel + '}}">');
                elementarray.push('<i class="fa fa-download"></i>');
                elementarray.push('</a>');
                elementarray.push('<a ');
                elementarray.push(' class="col-sm-4 btn btn-success btn-rad btn-trans ' + btnstyle + ' file-upload" target="_blank" href="File/Show/{{' + primaryinstitutionid + '}}/{{' + filemodel + '}}">');
                elementarray.push('<i class="fa fa-file-image-o"></i>');
                elementarray.push('</a>');
                elementarray.push('<button ');
                elementarray.push(' class="col-sm-4 btn btn-danger ' + btnstyle + ' btn-rad btn-trans file-upload"');
                elementarray.push(' ng-click="RemoveImage(' + filemodel + ')">');
                elementarray.push('<i class="fa fa-trash-o"></i>');
                elementarray.push('</button>');
                elementarray.push('</div>');
            }

            // Step 1: parse HTML into DOM element
            var template = angular.element(elementarray.join(' '));
            // Step 2: compile the template
            var linkFn = $compile(template);
            // Step 3: link the compiled template with the scope.
            var element = linkFn(scope);
            // Step 4: Append to DOM (optional)
            roolelement.append(element);


            function Resize(file, index, type) {
                var canvas = document.createElement("canvas");
                var img = document.createElement("img");
                var reader = new FileReader();
                reader.onload = function (e) {
                    img.src = e.target.result;
                    draw();
                };
                reader.readAsDataURL(file);

                function b64toBlob(b64Data, contentType, sliceSize) {
                    contentType = contentType || '';
                    sliceSize = sliceSize || 512;

                    var byteCharacters = atob(b64Data);
                    var byteArrays = [];

                    for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                        var slice = byteCharacters.slice(offset, offset + sliceSize);

                        var byteNumbers = new Array(slice.length);
                        for (var i = 0; i < slice.length; i++) {
                            byteNumbers[i] = slice.charCodeAt(i);
                        }

                        var byteArray = new Uint8Array(byteNumbers);

                        byteArrays.push(byteArray);
                    }

                    var blob = new Blob(byteArrays, { type: contentType });
                    return blob;
                }

                function draw() {
                    var width = img.width;
                    var height = img.height;
                    var ctx = canvas.getContext("2d");
                    ctx.drawImage(img, 0, 0);

                    if (width > 0 && height > 0) {
                        if (width > height) {
                            if (width > maxWidth) {
                                height *= maxWidth / width;
                                width = maxWidth;
                            }
                        } else {
                            if (height > maxHeight) {
                                width *= maxHeight / height;
                                height = maxHeight;
                            }
                        }

                        canvas.width = width;
                        canvas.height = height;
                        ctx.drawImage(img, 0, 0, width, height);
                        var b64 = canvas.toDataURL(type).split(',')[1];
                        file = b64toBlob(b64, type, 512);
                    }

                    uploadFile(file, index);
                }
            }

            function upload(fileProperties, index, file) {
                if (resize && maxWidth && maxHeight && (file.type.indexOf('image/') !== -1)) {
                    Resize(file, index, file.type);
                } else {
                    uploadFile(file, index);
                }
                var newUpload = angular.extend(scope.ngModel[index], {
                    name: fileProperties.name,
                    size: fileProperties.size,
                    type: fileProperties.type,
                    status: {},
                    deletable: false,
                    percent: 0,
                    preview: null
                });

                //$parse(filemodel).assign(scope, newUpload);

                return newUpload;
            }

            function uploadFile(file, index) {
                var xhr = new XMLHttpRequest(),
                    fd = new FormData(),
                    progress = 0,
                    uri = ((attr.uri) || ('File/UploadDirect/' + scope.UserInfo.OrganizationID + '/')) + filetype;
                xhr.open('POST', uri, true);
                xhr.withCredentials = shareCredentials;
                var authData = localStorageService.get('authorizationData');
                if (authData) {
                    xhr.setRequestHeader('Authorization', 'Bearer ' + authData.token);
                }

                if (scope.headers) {
                    scope.headers.forEach(function (item) {
                        xhr.setRequestHeader(item.header, item.value);
                    });
                }
                xhr.onreadystatechange = function () {
                    scope.ngModel[index].status = {
                        code: xhr.status,
                        statusText: xhr.statusText,
                        response: xhr.response
                    };
                    if (xhr.response != null && xhr.response != '') {
                        if (xhr.status == 417) {
                            if (xhr.readyState == 4 && xhr.response != '')
                                $rootScope.Toaster.Error(xhr.response);
                        }
                        else {
                            if (onuploadcomplete != null) {
                                if (xhr.status == 200 && xhr.readyState == 4) {
                                    try {
                                        if (typeof (filemodel) != 'undefined' || filemodel != null && filemodel.length > 0) {
                                            if (angular.isArray(JSON.parse(xhr.response)))
                                                $parse(filemodel).assign(scope, JSON.parse(xhr.response)[0]);
                                            else
                                                $parse(filemodel).assign(scope, JSON.parse(xhr.response));
                                        }
                                    } catch (exp) {

                                    }
                                    scope[onuploadcomplete](JSON.parse(xhr.response), scope);
                                }
                            }
                            else {
                                if (angular.isArray(JSON.parse(xhr.response)))
                                    $parse(filemodel).assign(scope, JSON.parse(xhr.response)[0]);
                                else
                                    $parse(filemodel).assign(scope, JSON.parse(xhr.response));
                                //$parse(filemodel).assign(scope, eval(xhr.response)[0]);
                            }
                        }
                    }
                    scope.$apply();
                };
                xhr.upload.addEventListener("progress", function (e) {
                    progress = parseInt(e.loaded / e.total * 100);
                    scope.ngModel[index].percent = progress;
                    scope.$apply();
                }, false);

                fd.append(fileName, file);
                xhr.send(fd);

                if (withPreview) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        scope.ngModel[index].preview = e.target.result;
                        scope.$apply();
                    };
                    reader.readAsDataURL(file);
                }
            }

            $timeout(function () {
                sel = fileSelector ? angular.element(el[0].querySelectorAll(fileSelector)[0]) : el;
                sel.bind('click', function () {
                    if (!scope.disabled) {
                        scope.$eval(el.find('input')[0].click());
                    }
                });
            });

            angular.element(el.find('input')[0]).bind('change', function (e) {
                var files = e.target.files;
                if (!angular.isDefined(scope.ngModel) || attr.multiple === 'true') {
                    scope.ngModel = [];
                }
                var f;
                if (onuploadstart && onuploadstart != null) scope[onuploadstart](scope);
                for (var i = 0; i < files.length; i++) {
                    f = {
                        name: files[i].name,
                        size: files[i].size,
                        type: files[i].type,
                        status: {},
                        percent: 0,
                        preview: null
                    };
                    scope.ngModel.push(f);
                    upload(f, i, files[i]);
                }
                scope.$apply();
            })
        }
    }
}]);



var Select2Adaptor = null;

jQuery.fn.select2.amd.require(["select2/data/array", "select2/utils"],

    function (ArrayData, Utils) {
        function CustomData($element, options) {
            CustomData.__super__.constructor.call(this, $element, options);
        }
        Utils.Extend(CustomData, ArrayData);

        CustomData.prototype.query = function (params, callback) {
            pageSize = 200;
            results = [];
            if (params.term && params.term !== '') {

                if (this.options.options.hasstartwith)
                    results = this.options.options._data.filter(function (e) {
                        return e.text.toUpperCase().startsWith(params.term.toUpperCase());
                    });
                else
                    results = this.options.options._data.filter(function (e) {
                        return e.text.toUpperCase().indexOf(params.term.toUpperCase()) >= 0;
                    });

            } else {
                results = this.options.options._data;
            }

            if (!("page" in params)) {
                params.page = 1;
            }

            var data = {};
            data.results = results.slice((params.page - 1) * pageSize, params.page * pageSize);

            if (data.results.length == 0 && this.options.options.tags) {
                var idfield = this.options.options.idfield;
                var displayfield = this.options.options.displayfield;
                var tlist = {};
                tlist[idfield] = '~'+params.term;
                tlist[displayfield] = params.term;
                tlist['text'] = params.term;
                data.results = [tlist];

                var existtags = this.options.options._data.find(function (item) {
                    return item[idfield] == '~' +params.term;
                });
                if (existtags && existtags != null) {
                    this.options.options._data.splice(this.options.options._data.indexOf(existtags), 1);
                }
            }

            data.pagination = {};
            data.pagination.more = params.page * pageSize < results.length;
            callback(data);
        };
        CustomData.prototype.setSelected = function (value) {
            var s = this.options.options._data.find(function (e) {
                return e.id == value;
            });
            if (s) return s;
            return { 'id': 0 };
        }
        Select2Adaptor = CustomData;
        //$(document).ready(function () {
        //    $("select").select2({
        //        ajax: {},
        //        dataAdapter: Select2Adaptor
        //    });
        //});
    })