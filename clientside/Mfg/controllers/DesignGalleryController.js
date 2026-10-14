
var app = angular.module('InfinityLab');

app.controller('DesignGalleryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.ImageWidth = 150;
        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });
        $scope.Classification = restService.Get('ReferencelistService/CLASSIFICATION', true).then(function (results) {
            $scope.Classification = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });
        $scope.Materials = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Materials = results;
        });
        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            results.push({ 'id': -9, 'name': 'All Retail Locations' });
            $scope.Locations = results;
        });
        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });
        $scope.SeasonList = restService.Get('ReferencelistService/SEASON', true).then(function (results) {
            $scope.SeasonList = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.SizeGroups = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroups = results;
        });

        var CondtionType =
        {
            Range: 0,
            String: 1,
            Long: 2,
            Date: 3,
            Decmal: 4,
            DayRange: 5
        }

        var Roles =
        {
            All: 0,
            Sales: 1,
            BothStock: 2,
            WHStock: 3,
            RStock: 4
        }

        $scope.Search = function () {
            var searchCondition = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            };

            var srequest = JSON.stringify(searchCondition);
            $http({
                method: 'POST',
                url: "MfgReportService/SearchInGallery",
                data: srequest
            }).success(function (result) {
                var Report = {
                    'creteria': JSON.parse(srequest)
                };
                Report.Mode = 'View';
                Report.DetailPosition = $scope.DetailPosition;
                Report.RowCount = $scope.RowCount;
                Report.DesignWidth = $scope.DesignWidth;
                Report.ImageWidth = $scope.ImageWidth;
                Report.ImageQuality = $scope.ImageQuality;
                Report.datasource = result;
                $scope.Report = Report;
                $timeout(function () {
                    if ($scope.DetailPosition == '2')
                        $scope.ShowOrderResult(result, Report);
                    else
                        $scope.ShowImageResult(result, Report);
                }, 100);
            });
        }

        $scope.Report = null;
        $scope.migrationtimer = null;
        $scope.downloading = false;

        $scope.CancelDownloadPDF = function () {
            $scope.downloading = false;
            if ($scope.migrationtimer != null)
                $timeout.cancel($scope.migrationtimer);
        }

        $scope.CheckStatus = function (key) {
            restService.Get('MfgReportService/ImagePDFGenerationStatus/' + key).then(function (result) {
                if ($scope.downloading && !result.completed && !result.failed) {
                    $scope.migrationtimer = $timeout(function () {
                        $scope.CheckStatus(key);
                    }, 2000);
                }
                else if (result.completed) {
                    $rootScope.Download("MfgReportService/ImagePDFGeneration/" + key, 'images.pdf', 'application/pdf');
                    $scope.downloading = false;
                }
                else if (result.failed) {
                    $scope.downloading = false;
                    $rootScope.Toaster.Info(result.message);
                }
            });
        }

        $scope.$on(
            "$destroy",
            function (event) {
                if ($scope.migrationtimer != null)
                    $timeout.cancel($scope.migrationtimer);
            }
        );

        $scope.DownloadPDF = function () {

            if ($scope.migrationtimer != null)
                $timeout.cancel($scope.migrationtimer);

            $scope.downloading = false;
            request = {
                "item": $scope.Report,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/ImagePDFGenerationStart",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $scope.downloading = true;
                    $scope.CheckStatus(result);
                }
                else {
                    $rootScope.Toaster.Info(result);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowOrderResult = function (result, report) {

            var size = {
                'width': $('#gridResultContainer').width(), 'height': $('#gridResultContainer').height()
            }

            if ($scope.irgrid != null) {
                $scope.irgrid.destroy();
                $('#gridResultContainer').empty();
            }

            var row = report.RowCount ? parseInt(report.RowCount) : 4;
            var col = 1;// report.ColumnCount ? parseInt(report.ColumnCount) : 2;
            var rowheight = (size.height - 50) / row;
            var imageheight = rowheight / 4 * 3;
            var groupresults = [];
            var imageindex = 0;
            var urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';

            $scope.irgrid = new InfyGrid('gridResultContainer', '450px', '100%', true, true, true, 0);
            $scope.irgrid.options.rowHeight = 200;
            $scope.irgrid.options.showHeaderRow = true;

            var column = $scope.irgrid.AddCustomColumn('img', '', 200, function (item, col) {
                if (item.imgid && item.imgid != null && item.imgid != '') {
                    return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item.imgid + '"/></div>';
                }
                return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img src="clientside/dist/img/no-image.png"/></div>';
            });
            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
            $scope.irgrid.AddSimpleColumn('designitemcode', 'Design', 120);
            $scope.irgrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.Colors);


            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'Detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: $scope.irgrid.columns
                });


            $.each(result.sizelist, function (k, v) {

                $scope.irgrid.columns = [];

                column = $scope.irgrid.AddCustomColumn('size_' + v.id, 'Stk', 50, function (item, col) {
                    var sizestock = item.sizes.find(function (item) {
                        return item.id == col.sizeid;
                    });
                    if (sizestock && sizestock != null) {
                        return sizestock.stock;
                    }
                    return '';
                });
                column.sizeid = v.id;
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    var sizestock = item.sizes.find(function (item) {
                        return item.id == col.sizeid;
                    });
                    if (sizestock && sizestock != null) {
                        return sizestock.stock;
                    }
                    return 0;
                }

                var column = $scope.irgrid.AddColumn(
                    {
                        id: 'order_' + v.id, name: 'Ord', field: 'order_' + v.id,
                        width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                groupColumns.push(
                    {
                        id: 'Size-' + v.id,
                        name: v.name,
                        headerCssClass: 'text-center',
                        columns: $scope.irgrid.columns
                    });
            });

            $scope.irgrid.columns = groupColumns;
            $scope.irgrid.EnableTotal();
            $scope.irgrid.Initialize();

            $scope.irgrid.grid.onDblClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id.startsWith('img')) {
                    item = args.grid.getDataItem(args.row);
                    var link = document.createElement('a');
                    link.href = args.grid.getColumns()[args.cell].urlprefix + item.imgid;
                    link.target = '_blank';
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }
            });

            $scope.irgrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id.startsWith('order_')) {
                    var sizeid = args.grid.getColumns()[args.cell].sizeid;
                    var sizestock = item.sizes.find(function (item) {
                        return item.id == sizeid;
                    });
                    if (sizestock && sizestock != null) {
                        sizestock.order = item[args.grid.getColumns()[args.cell].id];
                    }
                    else {
                        item.sizes.push({ 'order': item[args.grid.getColumns()[args.cell].id], 'sizeid': sizeid });
                    }
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.irgrid.datasource = result.items;
            $scope.irgrid.UpdateData(result.items);
        }

        $scope.ShowImageResult = function (result, report) {

            var size = {
                'width': $('#gridResultContainer').width(), 'height': $('#gridResultContainer').height()
            }

            if ($scope.irgrid != null) {
                $scope.irgrid.destroy();
                $('#gridResultContainer').empty();
            }

            if (result == undefined ||
                result == null ||
                result.sizelist == undefined ||
                result.sizelist == null) {
                $rootScope.Toaster.Info('Zero records available');
                return;
            }

            var row = report.RowCount ? parseInt(report.RowCount) : 4;
            var col = 1;// report.ColumnCount ? parseInt(report.ColumnCount) : 2;
            var rowheight = (size.height - 50) / row;
            var imageheight = rowheight / 4 * 3;
            var groupresults = [];
            var imageindex = 0;
            var urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';

            $scope.irgrid = new InfyGrid('gridResultContainer', '450px', '100%', true, true, true, 1);
            $scope.irgrid.options.rowHeight = 200;
            $scope.irgrid.options.showHeaderRow = true;

            var column = $scope.irgrid.AddCustomColumn('img', '', 200, function (item, col) {
                if (item.imgid && item.imgid != null && item.imgid != '') {
                    return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item.imgid + '"/></div>';
                }
                return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img src="clientside/dist/img/no-image.png"/></div>';
            });
            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';
            $scope.irgrid.AddSimpleColumn('designitemcode', 'Design', 120);
            $scope.irgrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.Colors);

            $.each(result.sizelist, function (k, v) {
                column = $scope.irgrid.AddCustomColumn('size_' + k, v.name, 50, function (item, col) {
                    var sizestock = item.sizes.find(function (item) {
                        return item.id == col.sizeid;
                    });
                    if (sizestock && sizestock != null) {
                        return sizestock.stock;
                    }
                    return '';
                });
                column.sizeid = v.id;
                column.total = true;
                column.enablefilter = true;
                column.getValue = function (item, col) {
                    `1`
                    var sizestock = item.sizes.find(function (item) {
                        return item.id == col.sizeid;
                    });
                    if (sizestock && sizestock != null) {
                        return sizestock.stock;
                    }
                    return 0;
                }
            });

            $scope.irgrid.EnableTotal();
            $scope.irgrid.Initialize();

            $scope.irgrid.grid.onDblClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id.startsWith('img')) {
                    item = args.grid.getDataItem(args.row);
                    var link = document.createElement('a');
                    link.href = args.grid.getColumns()[args.cell].urlprefix + item.imgid;
                    link.target = '_blank';
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }
            });

            $scope.irgrid.datasource = result.items;
            $scope.irgrid.UpdateData(result.items);
        }
        $scope.panelheight = { 'height': '500px' };
        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 50) + 'px';
            }
        }, 100);
    }]);
