var app = angular.module('InfinityLab');
app.controller('WidgetsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter', 'ngDialog', '$compile',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter, ngDialog, $compile) {

        $scope.SearchRef = restService.SearchReference;

        $scope.SearchCondition = {
            locationid: 0,
            fromdate: new Date(),
            todate: new Date()
        };

        $scope.RefreshSingle = function () {
            $.each($scope.WidgetSingle, function (k, v) {
                $scope.RefreshData(v);
            })
        }

        $scope.RefreshData = function (item) {

            item.wgconfig.rptrequest.topcount = item.wgconfig.topcount;
            item.wgconfig.rptrequest.topfield = item.wgconfig.topfield;
            var request = {
                "item": item.wgconfig.rptrequest,
                "securitytoken": "String content"
            }
            request.item.widgetid = item.id;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/GenerateReportBuilder",
                data: srequest
            }).success(function (result) {
                $scope.DisplaySingleData(item, $scope.ConvertData2List(result));
            }).error(function (result) {
            });
        }

        $scope.Save = function (item) {
            var controlid = "#wget_" + item.id;
            var config = $(controlid).data("pivotUIOptions");
            var config_copy = JSON.parse(JSON.stringify(config));
            delete config_copy["aggregators"];
            delete config_copy["renderers"];
            item.wgconfig.pivotconfig = JSON.stringify(config_copy);
            item.reportformat = { 'format': JSON.stringify(item.wgconfig) }
            $scope.ShowWidgetConfiguration(item, false, function (item,ispreview) {

            });

        //    request = {
        //        "Item": item,
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);

        //    $http({
        //        method: 'POST',
        //        url: "ConfigurationService/Save",
        //        data: srequest
        //    }).success(function (result) {
        //        $scope.Edit(false, item, $scope.WidgetList);
        //        $scope.LoadPIVOT(item, item.data);
        //    });
        }

        $scope.Edit = function (edit, item, list) {
            var controlid = "#wget_" + item.id;

            if (edit) {
                $scope.ShowWidgetSingle = false;
                $.each(list, function (k, v) {
                    v.hide = true;
                });
                item.hide = false;
                item.edit = true;
                item.lastrowclass = item.rowclass;
                item.rowclass = 'col-md-12';
                var pivotconfig = JSON.parse(item.wgconfig.pivotconfig);
                $(controlid).css('height', '500px');
                $(controlid).pivotUI(item.data, pivotconfig, true);
            }
            else {
                $scope.ShowWidgetSingle = $scope.WidgetSingle && $scope.WidgetSingle.length > 0;
                $.each(list, function (k, v) {
                    v.hide = false;
                });
                item.edit = false;
                item.rowclass = item.lastrowclass;
                var config = $(controlid).data("pivotUIOptions");
                var config_copy = JSON.parse(JSON.stringify(config));
                delete config_copy["aggregators"];
                delete config_copy["renderers"];
                item.wgconfig.pivotconfig = JSON.stringify(config_copy);
                $(controlid).css('height', '250px');
                $scope.LoadPIVOT(item, item.data);
            }
        }

        $scope.ConfigureSingle = function (v) {
            if (v.wgconfig.width == 'FULL') {
                v.rowclass = 'col-md-12';
            }
            else if (v.wgconfig.width == 'HALF') {
                v.rowclass = 'col-md-6';
            }
            else if (v.wgconfig.width == 'QUARTER') {
                v.rowclass = 'col-md-3';
            }
            else if (v.wgconfig.width == 'TWO') {
                v.rowclass = 'col-md-2';
            }
            else {
                v.rowclass = 'col-md-1';
            }
        }

        $scope.LoadWidgets = function () {

            $scope.derivers = $.pivotUtilities.derivers;
            $scope.renderers = $.extend(
                $.pivotUtilities.renderers,
                $.pivotUtilities.plotly_renderers,
                $.pivotUtilities.d3_renderers
            );

            request = {
                "item": { 'key': 'WGCONFIG' },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {

                $scope.availableheight = (typeof ($rootScope.windowSize) != 'undefined' &&
                    typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
                $scope.availablewidth = (typeof ($rootScope.windowSize) != 'undefined' &&
                    typeof ($rootScope.windowSize.width) != 'undefined') ? $rootScope.windowSize.width : 0;
                $scope.halfheight = { 'height': $scope.availableheight > 0 ? (($scope.availableheight / 2) + 'px') : '150px' };
                var fulllist = [];
                var singlelist = [];
                var customlist = [];
                var singlebg = ['bg-info', 'bg-success', 'bg-teal', 'bg-violet', 'bg-pink', 'bg-blue'];
                
                $.each(result, function (k, v) {
                    if (v.wgconfig.displaymode == 'DUAL' ||
                        v.wgconfig.displaymode == 'SINGLE') {
                        $scope.ConfigureSingle(v);
                        singlelist.push(v);
                        v.bgclass = singlebg[singlelist.length % 6];
                        $scope.LoadSingleData(v, true);
                    }
                    else if (v.wgconfig.displaymode == 'PLUGIN') {
                        var exist = customlist.find(function (item) {
                            return item.name == v.name;
                        });
                        if (exist == undefined || exist == null) {
                            customlist.push(v);
                            $scope.RunCustom(v.name, false, v);
                        }
                    }
                    else {
                        fulllist.push(v);
                        $scope.DisplayWidgetData(v);
                        $scope.LoadData(v, true);
                    }
                })

                $scope.WidgetList = fulllist;
                $scope.ShowWidgetSingle = false;
                if (singlelist.length > 0) {
                    $scope.ShowWidgetSingle = true;
                    $scope.WidgetSingle = singlelist;
                }
            });
        }

        $scope.DisplayWidgetData = function (v) {
            if (v.wgconfig.width == 'FULL') {
                v.rowclass = 'col-md-12';
                v.rowwidth = $scope.availablewidth;
            }
            else if (v.wgconfig.width == 'HALF') {
                v.rowclass = 'col-md-6';
                v.rowwidth = ($scope.availablewidth - 100) / 2;
            }
            else {
                v.rowclass = 'col-md-3';
                v.rowwidth = $scope.availablewidth / 4;
            }

            if (v.wgconfig.height == 'FULL')
                v.rowheight = $scope.availableheight;
            else if (v.wgconfig.height == 'HALF')
                v.rowheight = ($scope.availableheight - 100) / 2;
            else
                v.rowheight = $scope.availableheight / 4;
        }

        $scope.DisplaySingleData = function (item, results) {
            item.data = results;
            if (item.wgconfig.displaymode == 'DUAL') {
                var singlevalue = item.wgconfig.rptrequest.aggregatelist.find(function (fitem) {
                    return fitem.name == item.wgconfig.firstvalue;
                });
                item.topvalue = 0;
                item.bottomvalue = 0;
                if (singlevalue && singlevalue != null && results &&
                    results && results.length > 0) {
                    item.topvalue = Math.round(results[0][singlevalue.name]);
                }
                var secondvalue = item.wgconfig.rptrequest.aggregatelist.find(function (fitem) {
                    return fitem.name == item.wgconfig.secondvalue;
                });
                if (secondvalue && secondvalue != null && results &&
                    results.length > 0) {
                    item.bottomvalue = Math.round(results[0][secondvalue.name]);
                }

                if (item.topvalue > 10000)
                    item.topvalue = Math.round(item.topvalue / 1000).toString() + "k";
                if (item.bottomvalue > 10000)
                    item.bottomvalue = Math.round(item.bottomvalue / 1000).toString() + "k";
            }
            else {
                item.singlevalue = 0;
                var singlevalue = item.wgconfig.rptrequest.aggregatelist.find(function (fitem) {
                    return fitem.name == item.wgconfig.firstvalue;
                });
                if (singlevalue && singlevalue != null && results &&
                    results.length > 0) {
                    item.singlevalue = Math.round(results[0][singlevalue.name]);
                }
                if (item.singlevalue > 10000)
                    item.singlevalue = Math.round(item.singlevalue / 1000).toString() + "k";
            }
        }

        $scope.LoadSingleData = function (item) {
            var hasData = false;
            if (item.wgconfig.data && item.wgconfig.data != null) {
                var results = JSON.parse(item.wgconfig.data);
                hasData = true;
                $scope.DisplaySingleData(item, results);
            }

            restService.Get('File/WidgetData/' + $rootScope.UserInfo.OrganizationID + '/' + item.id, true).then(function (results) {
                if (typeof (results) == 'string') {
                    if (!hasData) {
                        $scope.RefreshData(item);
                    }
                    return;
                }
                var list = [];
                $.each(results, function (k, v) {
                    var rowitem = {};
                    $.each(item.wgconfig.rptrequest.aggregatelist, function (kk, vv) {
                        rowitem[vv.name] = v.Amount[vv.index];
                    });
                    list.push(rowitem);
                })
                $scope.DisplaySingleData(item, list);
            });

        }

        $scope.ConvertData2List = function (currentresult) {

            var columns = [];
            var rows = [];
            var cols = [];
            $.each(currentresult.columnlist, function (k, v) {
                columns.push({ 'type': 'columns', 'id': 'col_' + v.index, 'value': v, 'name': v.name });
                if (v == 0)
                    cols.push(v.name);
                else
                    rows.push(v.name);
            });

            $.each(currentresult.rangelist, function (k, v) {
                columns.push({ 'type': 'range', 'id': 'rng_' + v.index, 'value': v, 'name': v.name });
                rows.push(v.name);
            });

            var list = [];
            $.each(currentresult.result, function (k, v) {
                var row = {};
                $.each(currentresult.columnlist, function (kk, vv) {
                    row[vv.name] = v.String[vv.index];
                });
                $.each(currentresult.rangelist, function (kk, vv) {
                    row[vv.name] = v.RangeResult[vv.index];
                });
                $.each(currentresult.aggregatelist, function (kk, vv) {
                    row[vv.name] = v.Amount[vv.index];
                });
                list.push(row);
            });
            return list;
        }

        $scope.LoadData = function (item, tryserver) {
            var hasdata = false;
            if (item.wgconfig.data && item.wgconfig.data != null) {
                var list = JSON.parse(item.wgconfig.data);
                $scope.LoadPIVOT(item, list);
                hasdata = true
            }
            if (!hasdata && item.wgconfig.datastorage == 'STATIC') {
                $scope.CheckLatest(item, !hasdata);
            }
            else if (tryserver &&
                item.wgconfig.datastorage =='REALTIME') {
                $scope.CheckLatest(item, !hasdata);
            }
        }

        $scope.CheckLatest = function (item, tryfornone) {
            restService.Get('File/WidgetData/' + $rootScope.UserInfo.OrganizationID + '/' + item.id, true).then(function (results) {
                if (results && typeof (results) != 'string' && results != null) {
                    var list = [];
                    $.each(results, function (k, v) {
                        var rowitem = {};

                        if (item.wgconfig.rptrequest.aggregatelist &&
                            item.wgconfig.rptrequest.aggregatelist != null) {
                            $.each(item.wgconfig.rptrequest.aggregatelist, function (kk, vv) {
                                rowitem[vv.name] = v.Amount[vv.index];
                            });
                        }

                        if (item.wgconfig.rptrequest.columnlist &&
                            item.wgconfig.rptrequest.columnlist != null) {
                            $.each(item.wgconfig.rptrequest.columnlist, function (kk, vv) {
                                rowitem[vv.name] = v.String[vv.index];
                            });
                        }
                        if (item.wgconfig.rptrequest.rangelist) {
                            $.each(item.wgconfig.rptrequest.rangelist, function (kk, vv) {
                                rowitem[vv.name] = v.RangeResult[vv.index];
                            });
                        }
                        list.push(rowitem);
                    })
                    $scope.LoadPIVOT(item, list);
                }
                else if (tryfornone) {
                    $scope.Refresh(item);
                }
            });
        }

        $scope.Delete = function (item, list) {
            var request = {
                "Item": { 'id': item.id, 'version': item.version },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ConfigurationService/Delete",
                data: srequest
            }).success(function (result) {
                list.splice(list.indexOf(item), 1);
            });
        }

        $scope.LoadPIVOT = function (item, list) {
            var pivotconfig = JSON.parse(item.wgconfig.pivotconfig);
            item.data = list;
            $timeout(function () {
                var controlid = "#wget_" + item.id;
                if (pivotconfig.rendererOptions == undefined)
                    pivotconfig.rendererOptions = {};

                if (pivotconfig.rendererOptions.plotly == undefined)
                    pivotconfig.rendererOptions.plotly = {};

                pivotconfig.rendererOptions.plotly = {
                    width: item.rowwidth,
                    height: item.rowheight,
                    title: '',
                    title_text: '',
                    title_x: 0.5,
                    margin: { l: 10, r: 10, t: 10, b: 10 }
                };

                $(controlid).pivotUI(list, pivotconfig, true);
                var rows = $(controlid + " tbody").children();
                if (rows.length >= 3) {
                    $timeout(function () {
                        var menu = $(controlid + " .modebar");
                        menu.hide();
                        $(rows[0]).hide();
                        $(rows[1]).hide();
                        $($(rows[2]).children()[0]).hide();
                    }, 25);
                }
            }, 25);
        }

        $scope.BusinessTrandTimer = null;
        $scope.$on("$destroy", function () {
            if ($scope.BusinessTrandTimer != null)
                $timeout.cancel($scope.BusinessTrandTimer);
        });

        $scope.Refresh = function (item) {
            item.wgconfig.rptrequest.topcount = item.wgconfig.topcount;
            item.wgconfig.rptrequest.topfield = item.wgconfig.topfield;
            var request = {
                "item": item.wgconfig.rptrequest,
                "securitytoken": "String content"
            }
            request.item.widgetid = item.id;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/GenerateReportBuilder",
                data: srequest
            }).success(function (result) {
                $scope.ShowData(item, result);
            }).error(function (result) {
            });
        }

        $scope.ShowData = function (config, currentresult) {
            if (currentresult == undefined || currentresult == null) return;
            var list = $scope.ConvertData2List(currentresult);
            $scope.LoadPIVOT(config, list);
        }

        $scope.EditSingle = function (item) {
            $scope.ShowWidgetConfiguration(item, true, function (item, ispreview) {

            });
        }

        $scope.ShowWidgetConfiguration = function (item,issingle, callback) {

            var dialog = ngDialog.open({
                template: 'SaveWidget',
                data: {
                    'parent': $scope,
                    'item': item,
                    'restService': restService
                },
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.StorageModes = [{ 'code': 'STATIC', 'name': 'STATIC' },
                    { 'code': 'MANUAL', 'name': 'MANUAL' },
                    { 'code': 'REALTIME', 'name': 'REALTIME' },
                    { 'code': 'PERIODIC', 'name': 'PERIODIC' }];

                    $scope.DisplayModes = [{ 'code': 'LIST', 'name': 'LIST' },
                    { 'code': 'CHART', 'name': 'CHART' },
                    { 'code': 'DUAL', 'name': 'DUAL' },
                    { 'code': 'SINGLE', 'name': 'SINGLE' }];

                    $scope.SIZES = [{ 'code': 'FULL', 'name': 'FULL' },
                    { 'code': 'HALF', 'name': 'HALF' },
                    { 'code': 'QUARTER', 'name': 'QUARTER' }];

                    $scope.SINGLESIZES = [{ 'code': 'FULL', 'name': 'FULL' },
                    { 'code': 'HALF', 'name': 'HALF' },
                    { 'code': 'QUARTER', 'name': 'QUARTER' },
                    { 'code': 'TWO', 'name': 'TWO' },
                    { 'code': 'SINGLE', 'name': 'SINGLE' }
                    ];

                    $scope.parent = $scope.ngDialogData.parent;
                    $scope.item = $scope.ngDialogData.item;
                    $scope.aggregatelist = JSON.parse(JSON.stringify(item.wgconfig.rptrequest.aggregatelist));
                    $scope.name = item.name;
                    $scope.title = item.wgconfig.title;
                    $scope.datastorage = item.wgconfig.datastorage;
                    $scope.displaymode = item.wgconfig.displaymode;
                    $scope.accessall = item.wgconfig.accessall;
                    $scope.width = item.wgconfig.width;
                    $scope.height = item.wgconfig.height;
                    $scope.firstvalue = item.wgconfig.firstvalue;
                    $scope.secondvalue = item.wgconfig.secondvalue;
                    $scope.userids = item.wgconfig.userids;
                    $scope.topcount = item.wgconfig.topcount;
                    $scope.topfield = item.wgconfig.topfield;

                    $scope.ngDialogData.restService.Get('ReferencelistService/USERLIST', true).then(function (results) {
                        $scope.UserList = results;
                        $.each(results, function (k, v) {
                            var exist = $scope.userids.find(function (item) {
                                return item == v.id;
                            });
                            v.access = (exist && exist != null);
                        })
                    });

                    $scope.Delete = function () {
                        $scope.closeThisDialog({ 'delete': true });
                    }

                    $scope.Select = function (preview) {
                        var response = {
                            'name': $scope.name,
                            'preview': preview,
                            'title': $scope.title,
                            'datastorage': $scope.datastorage,
                            'displaymode': $scope.displaymode,
                            'accessall': $scope.accessall,
                            'width': $scope.width,
                            'topcount': $scope.topcount,
                            'topfield': $scope.topfield,
                            'firstvalue': $scope.firstvalue,
                            'secondvalue': $scope.secondvalue,
                            'height': $scope.height,
                            'userids': []
                        };
                        $.each($scope.UserList, function (k, v) {
                            if (v.access)
                                response.userids.push(v.id);
                        })
                        $scope.closeThisDialog(response);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if (data.value.delete) {
                            $scope.Delete(item, $scope.WidgetSingle);
                        } else if (data.value.preview) {
                            item.wgconfig.name = data.value.name;
                            item.wgconfig.title = data.value.title;
                            item.wgconfig.datastorage = data.value.datastorage;
                            item.wgconfig.displaymode = data.value.displaymode;
                            item.wgconfig.accessall = data.value.accessall;
                            item.wgconfig.width = data.value.width;
                            item.wgconfig.height = data.value.height;
                            item.wgconfig.topcount = data.value.topcount;
                            item.wgconfig.topfield = data.value.topfield;
                            item.wgconfig.firstvalue = data.value.firstvalue;
                            item.wgconfig.secondvalue = data.value.secondvalue;
                            if (issingle) {
                                $scope.DisplaySingleData(item, item.data);
                                $scope.ConfigureSingle(item);
                            }
                            else {
                                item.wgconfig.name = data.value.name;
                                item.wgconfig.title = data.value.title;
                                item.wgconfig.datastorage = data.value.datastorage;
                                item.wgconfig.displaymode = data.value.displaymode;
                                item.wgconfig.accessall = data.value.accessall;
                                item.wgconfig.width = data.value.width;
                                item.wgconfig.height = data.value.height;
                                item.wgconfig.topcount = data.value.topcount;
                                item.wgconfig.topfield = data.value.topfield;
                                item.wgconfig.firstvalue = data.value.firstvalue;
                                item.wgconfig.secondvalue = data.value.secondvalue;
                                $scope.DisplayWidgetData(item);
                                $scope.Edit(false, item, $scope.WidgetList);
                                $scope.LoadPIVOT(item, item.data);
                            }
                            callback(item,true);
                        }
                        else {
                            var wgconfig = {
                                'title': data.value.title,
                                'name': data.value.name,
                                'datastorage': data.value.datastorage,
                                'displaymode': data.value.displaymode,
                                'userids': data.value.userids,
                                'width': data.value.width,
                                'firstvalue': data.value.firstvalue,
                                'secondvalue': data.value.secondvalue,
                                'height': data.value.height,
                                'topcount': data.value.topcount,
                                'topfield': data.value.topfield,
                                'accessall': data.value.accessall,
                                'pivotconfig': item.wgconfig.pivotconfig,
                                'rptrequest': item.wgconfig.rptrequest,
                                'data': data.value.datastorage == 'STATIC' ? JSON.stringify(item.data) : null
                            };

                            var configuration = {
                                'id': item.id,
                                'name': data.value.name,
                                'key': 'WGCONFIG',
                                'isactive': true,
                                'createdby': item.createdby,
                                'modifiedby': item.modifiedby,
                                'createdon': item.createdon,
                                'modifiedon': item.modifiedon,
                                'reportformat': { 'format': JSON.stringify(wgconfig) }
                            };
                            request = {
                                "Item": configuration,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "ConfigurationService/Save",
                                data: srequest
                            }).success(function (result) {
                                if (issingle) {
                                    $scope.DisplaySingleData(item, item.data);
                                    $scope.ConfigureSingle(item);
                                }
                                else {
                                    item.wgconfig.name = data.value.name;
                                    item.wgconfig.title = data.value.title;
                                    item.wgconfig.datastorage = data.value.datastorage;
                                    item.wgconfig.displaymode = data.value.displaymode;
                                    item.wgconfig.accessall = data.value.accessall;
                                    item.wgconfig.width = data.value.width;
                                    item.wgconfig.height = data.value.height;
                                    item.wgconfig.topcount = data.value.topcount;
                                    item.wgconfig.topfield = data.value.topfield;
                                    item.wgconfig.firstvalue = data.value.firstvalue;
                                    item.wgconfig.secondvalue = data.value.secondvalue;
                                    $scope.DisplayWidgetData(item);
                                    $scope.Edit(false, item, $scope.WidgetList);
                                    $scope.LoadPIVOT(item, item.data);
                                }
                                callback(item,false);
                            });
                        }
                    }
                }
            });
        }

        $scope.halfheight = { "height": '0px', "width": '0px' };
        $scope.panelheight = { "height": '0px', "width": '0px' };
        $scope.CustomWidgetList = [];
        $scope.CustomWidgetListID = 0;
        $scope.EditCustomWidget = false;

        $scope.SaveCustom = function () {

        }

        $scope.Template = $('#WidgetTemplate').html();
        $scope.Test = function () {

            //restService.Get('AnalyserService/LocationSummary', true).then(function (results) {

            //    var rows = [];
            //    var cols = [];
            //    rows.push('name');

            //    var renderers = $.extend(
            //        $.pivotUtilities.renderers,
            //        $.pivotUtilities.plotly_renderers,
            //        $.pivotUtilities.d3_renderers
            //    );

            //    var availableheight = (typeof ($rootScope.windowSize) != 'undefined' &&
            //        typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            //    var availablewidth = (typeof ($rootScope.windowSize) != 'undefined' &&
            //        typeof ($rootScope.windowSize.width) != 'undefined') ? $rootScope.windowSize.width : 0;

            //    if (availableheight > 0) {
            //        $scope.panelheight.height = '' + (availableheight - 50) + 'px';
            //    }

            //    if (availablewidth > 0) {
            //        $scope.panelheight.width = '' + (availablewidth - 50) + 'px';
            //    }

            //    $('#wget_custom').empty();
            //    $timeout(function () {
            //        $("#wget_custom").pivotUI(results.items, {
            //            renderers: renderers,
            //            cols: cols, rows: rows,
            //            rendererName: "Table Barchart",
            //            rowOrder: "value_a_to_z", colOrder: "value_z_to_a",
            //        });
            //    }, 10);




            //    //var wg = {
            //    //    'rowclass': 'col-md-6',
            //    //    'wgconfig': {
            //    //        'title': 'Test',
            //    //        'name': 'Test',
            //    //        'datastorage': 'STATIC',
            //    //        'displaymode': 'CHART',
            //    //        'width': 'HALF',
            //    //        'height': 'HALF',
            //    //        'accessall': true
            //    //    }
            //    //};
            //    //$scope.CustomWidgetListID++;
            //    //wg.id = 'ID_' + $scope.CustomWidgetListID;
            //    //var template = $scope.Template.replace('[0]', wg.id)
            //    //angular.element($("#widgetbox")).append($compile(template)($scope));
            //});
            //stocksalessummary.Run($scope);
        }

        $scope.DeleteCustom = function (widget) {
            var request = {
                "Item": { 'id': widget.item.id, 'version': widget.item.version },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ConfigurationService/Delete",
                data: srequest
            }).success(function (result) {
                widget.hide = true;
                $scope[widget.ID] = false;
                $('#wgetheader_' + widget.id).remove();
            });
        }

        $scope.RefreshCustom = function (widget) {
            if (widget.Refresh)
                widget.Refresh();
        }

        $scope.RunCustom = function (report, save,configitem) {
            if ($scope[report] && $scope[report] != null) return;

            var title = '';
            var wg = null;
            switch (report) {
                case 'stocksalessummary':
                    stocksalessummary.Run($scope);
                    wg = stocksalessummary;
                    title = 'Stock / Sales Summary';
                    break;
                case 'locationpricesummary':
                    locationpricesummary.Run($scope);
                    wg = locationpricesummary;
                    title = 'Location Price Summary';
                    break;
                case 'locationagesummary':
                    locationagesummary.Run($scope);
                    wg = locationagesummary;
                    title = 'Location Age Summary';
                    break;
                case 'locationsalesummary':
                    locationsalesummary.Run($scope);
                    wg = locationsalesummary;
                    title = 'Location Sales Summary';
                    break;
            }

            if (save) {
                var item = {
                    'name': report,
                    'key': 'WGCONFIG',
                    'isactive': true,
                    'userid': $rootScope.UserInfo.id,
                    'wgconfig': {
                        'name': report,
                        'title': title,
                        'displaymode': 'PLUGIN'
                    }
                };
                item.reportformat = { 'format': JSON.stringify(item.wgconfig) }
                request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ConfigurationService/Save",
                    data: srequest
                }).success(function (result) {
                    wg.item = result;
                });
            }
            else if(wg && wg !=null) {
                wg.item = configitem;
            }
        }

        $timeout(function () {
            $scope.LoadWidgets();
        }, 50);

        var stocksalessummary = {
            ID: 'stocksalessummary',
            Mode: 'List',
            rowclass: 'col-md-6',
            title: 'Stock/Sales Summary',
            Parameter: ['LOCATION'],
            Refresh: function () {
                var localscope = this;
                restService.Get('AnalyserService/StockSaleInsight', true).then(function (results) {
                    localscope.grid.UpdateData(results);
                });
            },
            Run: function (parentscope) {
                var localscope = this;
                restService.Get('AnalyserService/StockSaleInsight', true).then(function (results) {
                    localscope.Render(parentscope, results);
                });
            },
            Render: function (parentscope, results) {
                var localscope = this;
                localscope.data = results;
                parentscope.stocksalessummary = true;
                localscope.id = 'ID_' + parentscope.CustomWidgetListID.toString();
                var template = parentscope.Template.replaceAll('[0]', '.' + localscope.id)
                    .replaceAll('[ID]', localscope.id);

                parentscope.CustomWidgetList[localscope.id] = localscope;
                parentscope.CustomWidgetListID++;
                angular.element($('#widgetbox')).append($compile(template)(parentscope));
                localscope.hide = false;
                $timeout(function () {
                    var height = ((parentscope.availableheight - 100) / 2) + 'px';
                    var igrid = new InfyGrid('wget_' + localscope.id, height, '100%', false, true, true);
                    igrid.options.showHeaderRow = true;
                    igrid.AddSimpleColumn('name', 'Product', 80);
                    igrid.AddFormatedColumn('openingqty', 'Op.Qty', 80, 'N0', 0);
                    igrid.AddFormatedColumn('openingvalue', 'Op.Value', 80, 'N0', 0);
                    igrid.AddFormatedColumn('purchaseqty', 'P.Qty', 80, 'N0', 0);
                    igrid.AddFormatedColumn('purchasevalue', 'P.Value', 80, 'N0', 0);
                    igrid.AddFormatedColumn('soldqty', 'S.Qty', 80, 'N0', 0);
                    igrid.AddFormatedColumn('soldvalue', 'S.Value', 80, 'N0', 0);
                    igrid.AddFormatedColumn('closingqty', 'Cl.Qty', 80, 'N0', 0);
                    igrid.AddFormatedColumn('closingvalue', 'Cl.Value', 80, 'N0', 0);
                    igrid.AddFormatedColumn('clratio', '%', 40, 'N2', 2);
                    igrid.AddFormatedColumn('sellthrogh', 'Sell Through', 80, 'N2', 2);
                    igrid.AddFormatedColumn('stockturn', 'Stock Turn', 80, 'N2', 2);
                    igrid.AddSimpleColumn('test', '', 20);
                    igrid.Initialize();
                    igrid.UpdateData(localscope.data);
                    localscope.grid = igrid;
                }, 10)
            }
        }

        var locationpricesummary = {
            ID: 'locationpricesummary',
            Mode: 'List',
            Parameter: ['LOCATION'],
            rowclass: 'col-md-6',
            title: 'Price Summary',
            Refresh: function () {
                var localscope = this;
                restService.Get('AnalyserService/LocationPrices', true).then(function (results) {
                    localscope.grid.UpdateData(results);
                });
            },
            Run: function (parentscope) {
                var localscope = this;

                restService.Get('AnalyserService/LocationPrices', true).then(function (results) {
                    localscope.Render(parentscope, results);
                });
            },
            Render: function (parentscope, results) {
                var localscope = this;
                localscope.data = results;
                parentscope.locationpricesummary = true;
                localscope.id = 'ID_' + parentscope.CustomWidgetListID.toString();
                var template = parentscope.Template.replaceAll('[0]', '.' + localscope.id)
                    .replaceAll('[ID]', localscope.id);

                parentscope.CustomWidgetList[localscope.id] = localscope;
                parentscope.CustomWidgetListID++;
                angular.element($('#widgetbox')).append($compile(template)(parentscope));
                var localscope = this;
                localscope.hide = false;
                $timeout(function () {
                    var height = ((parentscope.availableheight - 100) / 2) + 'px';
                    var igrid = new InfyGrid('wget_' + localscope.id, height, '100%', false, true, true);
                    igrid.options.showHeaderRow = true;
                    igrid.AddSimpleColumn('price', 'Price', 80);
                    igrid.AddFormatedColumn('stockqty', 'Stock', 80, 'N0', 0);
                    igrid.AddFormatedColumn('stockvalue', 'Cost', 80, 'N0', 0);
                    igrid.AddFormatedColumn('saleqty', 'Sold', 80, 'N0', 0);
                    igrid.AddFormatedColumn('salesvalue', 'Value', 80, 'N0', 0);
                    igrid.AddFormatedColumn('purchaseqty', 'Purchase', 80, 'N0', 0);
                    igrid.AddFormatedColumn('purchasevalue', 'Cost', 80, 'N0', 0);
                    igrid.AddFormatedColumn('pt', '%', 40, 'N2', 2);
                    igrid.AddFormatedColumn('rt', 'Turns', 50, 'N2', 2);
                    igrid.Initialize();
                    igrid.UpdateData(localscope.data);
                    localscope.grid = igrid;
                }, 10);

            }
        }


        var locationagesummary = {
            ID: 'locationagesummary',
            Mode: 'List',
            rowclass: 'col-md-6',
            title: 'Age Summary',
            Parameter: ['LOCATION'],
            Refresh: function () {
                var localscope = this;
                restService.Get('AnalyserService/Locationstock', true).then(function (results) {
                    localscope.grid.UpdateData(results);
                });
            },
            Run: function (parentscope) {
                var localscope = this;
                restService.Get('AnalyserService/Locationstock', true).then(function (results) {
                    localscope.Render(parentscope, results);
                });
            },

            Render: function (parentscope, results) {
                var localscope = this;
                localscope.data = results;
                parentscope.locationagesummary = true;

                localscope.id = 'ID_' + parentscope.CustomWidgetListID.toString();
                var template = parentscope.Template.replaceAll('[0]', '.' + localscope.id)
                    .replaceAll('[ID]', localscope.id);

                parentscope.CustomWidgetList[localscope.id] = localscope;
                parentscope.CustomWidgetListID++;
                angular.element($('#widgetbox')).append($compile(template)(parentscope));
                var localscope = this;
                $timeout(function () {
                    var height = ((parentscope.availableheight - 100) / 2) + 'px';
                    var igrid = new InfyGrid('wget_' + localscope.id, height, '100%', false, true, true);
                    igrid.options.showHeaderRow = true;
                    igrid.AddSimpleColumn('Period', 'Days', 80);
                    igrid.AddFormatedColumn('Qty', 'Stock', 100, 'N0', 0);
                    igrid.AddFormatedColumn('Value', 'Cost', 100, 'N0', 0);
                    igrid.AddFormatedColumn('Sold', 'Sold', 100, 'N0', 0);
                    igrid.AddFormatedColumn('TSold', 'Q-Sale', 100, 'N0', 0);
                    igrid.AddFormatedColumn('pt', '%', 40, 'N2', 2);
                    igrid.Initialize();
                    igrid.UpdateData(localscope.data);
                    localscope.hide = false;
                    localscope.grid = igrid;
                }, 10);
            }
        }


        var locationsalesummary = {
            ID: 'locationsalesummary',
            Mode: 'List',
            rowclass : 'col-md-6',
            title: 'Location Sale Summary',
            Parameter: ['LOCATION'],
            Refresh: function () {
                var localscope = this;
                restService.Get('AnalyserService/LocationSummary', true).then(function (results) {
                    localscope.grid.UpdateData(results.items);
                });
            },
            Run: function (parentscope) {
                var localscope = this;
                restService.Get('AnalyserService/LocationSummary', true).then(function (results) {
                    localscope.Render(parentscope, results);
                });
            },
            Render: function (parentscope, results) {
                var localscope = this;
                localscope.data = results;
                parentscope.locationsalesummary = true;
                localscope.id = 'ID_' + parentscope.CustomWidgetListID.toString();
                var template = parentscope.Template.replaceAll('[0]', '.' + localscope.id)
                    .replaceAll('[ID]', localscope.id);
                
                parentscope.CustomWidgetList[localscope.id] = localscope;
                parentscope.CustomWidgetListID++;
                angular.element($('#widgetbox')).append($compile(template)(parentscope));
                localscope.hide = false;
                $timeout(function () {
                    var height = ((parentscope.availableheight - 100) / 2) + 'px';
                    var igrid = new InfyGrid('wget_' + localscope.id, height, '100%', false, true, true);
                    igrid.options.showHeaderRow = true;
                    igrid.AddSimpleColumn('name', 'Location', 80);
                    igrid.AddFormatedColumn('stock', 'Stock', 100, 'N0', 0);
                    igrid.AddFormatedColumn('todaysales', 'Today', 100, 'N0', 0);
                    igrid.AddFormatedColumn('currentmonth', 'Month', 100, 'N0', 0);
                    igrid.AddFormatedColumn('lastyearmonth', 'Last Month', 100, 'N0', 0);

                    var column = igrid.AddCustomColumn('today-abs', 'ABS-T', 50, function (item, col) {
                        if (item.todaysalesqty > 0 && item.todaybillcount > 0)
                            return (item.todaysalesqty / item.todaybillcount).toFixed(2);
                        return '';
                    });

                    column = igrid.AddCustomColumn('month-abs', 'ABS-M', 50, function (item, col) {
                        if (item.monthsalesqty > 0 && item.monthbillcount > 0)
                            return (item.monthsalesqty / item.monthbillcount).toFixed(2);
                        return '';
                    });

                    column = igrid.AddCustomColumn('last-abs', 'ABS-L', 50, function (item, col) {
                        if (item.lastyearmonthqty > 0 && item.lastyearmonthcount > 0)
                            return (item.lastyearmonthqty / item.lastyearmonthcount).toFixed(2);
                        return '';
                    });

                    var column = igrid.AddCustomColumn('today-atv', 'ATV-T', 50, function (item, col) {
                        if (item.todaysales > 0 && item.todaybillcount > 0)
                            return (item.todaysales / item.todaybillcount).toFixed(2);
                        return '';
                    });

                    column = igrid.AddCustomColumn('month-atv', 'ATV-M', 50, function (item, col) {
                        if (item.currentmonth > 0 && item.monthbillcount > 0)
                            return (item.currentmonth / item.monthbillcount).toFixed(2);
                        return '';
                    });

                    column = igrid.AddCustomColumn('last-atv', 'ATV-L', 50, function (item, col) {
                        if (item.lastyearmonth > 0 && item.lastyearmonthcount > 0)
                            return (item.lastyearmonth / item.lastyearmonthcount).toFixed(2);
                        return '';
                    });
                    igrid.Initialize();
                    igrid.UpdateData(localscope.data.items);
                    localscope.grid = igrid;
                }, 10);
            }
        }

    }]);