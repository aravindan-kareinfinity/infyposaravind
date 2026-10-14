
var app = angular.module('InfinityLab');

app.controller('ItemSearchController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.system_isb2bapplication = false;
        $scope.system_posmultiprice = false;
        $scope.system_supportmrp = false;
        $scope.Appsetting_Company = false;
        $scope.Customers = [];
        $scope.LoadApplicationSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.Appsetting_Company = $rootScope.UserInfo.appsettings.system_multicompanytype && $rootScope.UserInfo.appsettings.system_multicompanytype == 1;
                $scope.system_isb2bapplication = $rootScope.UserInfo.appsettings.system_isb2bapplication;
                $scope.system_supportmrp = $rootScope.UserInfo.appsettings.system_salepricemrpdifference || false;
                $scope.system_posmultiprice = $rootScope.UserInfo.appsettings.system_posmultiprice > 0;
                if ($scope.system_isb2bapplication) {
                    $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
                        $scope.Customers = results;
                    });
                }
            }
            else {
                var configtimeout = $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50)
            }
        }



        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            results.push({ 'id': 0, 'name': 'Warehouse' });
            results.push({ 'id': -1, 'name': 'Warehouse' });
            $scope.Location = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.patterns = results;
        });

        $scope.sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.sizes = results;
        });

        $scope.materials = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.materials = results;
        });

        $scope.styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.styles = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.SearchResult = [];
        $scope.SearchRetailResult = [];
        $scope.result = { 'stock': null, 'lrinvoice': null, 'salablegoods': null, 'billitems': null };

        $scope.SearchCondition = {
            mode: 2,
            productid: 0,
            supplierid: 0,
            barcode: "",
            designid: ""
        }

        $scope.Moveto = function (action) {
            if (action == 'invoice')
                $location.url('/Invoice?lrid=' + $scope.result.lrinvoice.lrid);
            else if (action == 'product')
                $location.url('/LRInvoiceItems?invoiceid=' + $scope.result.lrinvoice.invoiceid);
            else if (action == 'barcode')
                $location.url('/barcode?invoiceid=' + $scope.result.lrinvoice.invoiceid);
        };

        $scope.GetTransferHistory = function () {
            var request = {
                "Item": { 'barcode': $scope.result.stock.barcode },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/TransferHistory",
                data: srequest
            }).success(function (result) {
                if ($scope.result != 'undefined' && $scope.result != null)
                    $scope.result.transferHistories = result;
            }).error(function (result) {
            });
        };

        $scope.ItemHistory = function (full) {

            if ($scope.Appsetting_Company &&
                (typeof ($scope.SearchCondition.companyid) == 'undefined'
                    || $scope.SearchCondition.companyid == null || $scope.SearchCondition.companyid < 0)) {
                $rootScope.Toaster.Info(" Please select company");
                return;
            }

            if ($scope.SearchCondition.searchmode == "2" && $scope.SearchCondition.barcode == "") {
                $rootScope.Toaster.Info("Please enter barcode");
                return;
            }

            if ($scope.SearchCondition.searchmode == "1" && $rootScope.LocationInfo == null) {
                $rootScope.Toaster.Info("Please Select Sale location");
                return;
            }

            $scope.SearchCondition.locationid = $scope.SearchCondition.searchmode == "1" ? $rootScope.LocationInfo.location.id : 0;
            $scope.SearchCondition.mode = ($scope.SearchCondition.searchmode == "0" || $scope.SearchCondition.searchmode == "3") ? 1 : $scope.SearchCondition.searchmode;

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/ItemHistory",
                data: srequest
            }).success(function (result) {
                $scope.result = result;
                if ($scope.SearchCondition.searchmode == '3') {
                    $scope.ShowImageDialog();
                }
                else {
                    if ($scope.SearchCondition.searchmode == '0' ||
                        $scope.SearchCondition.searchmode == '1' ||
                        $scope.SearchCondition.searchmode == '4') {
                        var billitems = [];
                        if (result.b2bbillitems != null) {
                            $.each(result.b2bbillitems, function (k, v) {
                                v.smode = 'B2B';
                                billitems.push(v);
                            })
                        }
                        if (result.billitems != null) {
                            $.each(result.billitems, function (k, v) {
                                v.smode = 'POS';
                                billitems.push(v);
                            })
                        }
                        $scope.ShowSoldBills(billitems);
                        //$scope.UserInfo.p_SALESREPORT.view 
                    }
                    $scope.SearchCondition.barcode = '';
                    $scope.setFocusfirst();
                }
            }).error(function (result) {
            });

        }


        $scope.ShowSoldBills = function (billitems) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 250);
            igrid.options.showHeaderRow = true;

            if ($scope.SearchCondition.searchmode != "1")
                igrid.AddSimpleColumn('location', 'Location', 100);

            igrid.AddSimpleColumn('barcode', 'Barcode', 80);
            igrid.AddSimpleColumn('billno', 'Bill No', 80);
            igrid.AddFormatedColumn('billdate', 'Bill Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('salesmancode', 'Sales Man Code', 80);
            igrid.AddSimpleColumn('salesmanname', 'Sales Man', 120);
            igrid.AddSimpleColumn('customer', 'Customer', 120);
            igrid.AddSimpleColumn('printingname', 'Product', 100);
            if ($scope.system_isb2bapplication) {
                igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.BrandName);
                igrid.AddSimpleColumn('designid', 'Design', 100);
            }
            igrid.AddFormatedColumn('rate', 'Rate', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('qty', 'Qty', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Disc', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('additionaldiscount', 'Addln.Dis', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tax', 'Tax', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivable', 'Amount', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('billvalue', 'Bill Value', 70, 'N2', 2).total = true;

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Sold'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Return'/> &nbsp;"

            if ($scope.SearchCondition.searchmode != "4") {
                igrid.AddButtonColumn('action', 'Image', 125, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Sold') {
                            $timeout(function () {
                                var link = document.createElement('a');
                                link.href = 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + args.item.stockimageid;
                                document.body.appendChild(link);
                                link.click();
                                link.remove();
                            }, 50);
                        }
                        else if ($(e.target).val() == 'Return') {
                            $timeout(function () {
                                var link = document.createElement('a');
                                link.href = 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + args.item.returnimageid;
                                document.body.appendChild(link);
                                link.click();
                                link.remove();
                            }, 50);
                        }
                    });
                });
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(billitems);
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'SALES LIST');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }

        $scope.qEncodeURIComponent = function (value) {
            if (value.indexOf('.') >= 0)
                value = value.replace('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replace('/', '~sls~');
            return encodeURIComponent(value);
        }

        $scope.ShowMultiPrice = function () {
            var dialog = ngDialog.open({
                template: 'MultiplePriceList',
                data: [$scope, $scope.result.stock.barcode, $scope.result.stock.serialno],
                width: '700px',
                controller: ['$scope', '$timeout', '$http', 'restService',
                    function ($scope, $timeout, $http, restService) {
                        $scope.Parent = $scope.ngDialogData[0];
                        $scope.Barcode = $scope.ngDialogData[1];
                        if ($scope.ngDialogData[2] && 
                            $scope.ngDialogData[2] != null &&
                            $scope.ngDialogData[2] != '')
                            $scope.Barcode = $scope.ngDialogData[2];

                        $scope.Results = [];

                        restService.Get('SalablegoodsService/BatchPriceList/' +
                            $scope.Parent.qEncodeURIComponent($scope.Barcode))
                            .then(function (results) {
                                $scope.Results = results;
                            });

                        $scope.Disable = function (item) {
                            item.isactive = false;
                            item.removedon = new Date();
                            var request = {
                                "Item": item,
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "SalablegoodsService/RemovePriceList",
                                data: srequest
                            }).success(function (result) {
                                $scope.closeThisDialog(result);
                            });
                        }

                        $scope.Save = function () {
                            if ($scope.locationid == undefined) return;
                            if ($scope.price == undefined) return;
                            if ($scope.mrp == undefined) return;
                            $scope.price = parseFloat($scope.price);
                            $scope.mrp = parseFloat($scope.mrp);
                            if (isNaN($scope.price)) return;
                            if (isNaN($scope.mrp)) return;
                            var locations = [];
                            if ($scope.locationid < 0) {
                                locations.push(0);
                            //    if ($scope.locationid == -2) {
                            //        $.each($scope.Parent.Location, function (k, v) {
                            //            if (v.id > 0) {
                            //                locations.push(v.id);
                            //            }
                            //        })
                            //    }
                            //    else {
                            //        return;
                            //    }
                            }
                            else {
                                locations.push($scope.locationid);
                            }

                            var barcode = $scope.Barcode;
                            if ($scope.Parent.result.stock.serialno &&
                                $scope.Parent.result.stock.serialno != null &&
                                $scope.Parent.result.stock.serialno != '') {
                                barcode = $scope.Parent.result.stock.serialno;
                            }
                            var request = {
                                "Item": {
                                    'locationids': locations,
                                    'price': $scope.price,
                                    'mrp': $scope.mrp,
                                    'barcode': barcode,
                                },
                                "SecurityToken": "String content"
                            }

                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "SalablegoodsService/SavePriceList",
                                data: srequest
                            }).success(function (result) {
                                $scope.closeThisDialog(result);
                            });
                        }
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {

                    }
                }
            });
        }
        $scope.ShowImageDialog = function () {
            var dialog = ngDialog.open({
                template: 'UpdateImage',
                data: [$scope],
                width: '660px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.Billitemid = 0;
                    $scope.ShowSnap = false;
                    $scope.Capture = function () {
                        var canvas = document.getElementById('canvas');
                        var context = canvas.getContext('2d');
                        var video = document.getElementById('video');
                        context.drawImage(video, 0, 0, 640, 480);
                        $scope.ShowSnap = true;
                    }

                    $scope.Save = function () {
                        var canvas = document.getElementById('canvas');
                        var imgData = canvas.toDataURL();
                        var request = {
                            "Item": {
                                'image': imgData, 'barcode': $scope.ngDialogData[0].SearchCondition.barcode,
                                'action': 2, 'billitemid': $scope.Billitemid
                            },
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "StockService/Image",
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

                        if (typeof ($scope.ngDialogData[0].result.billitems) != 'undefined' &&
                            $scope.ngDialogData[0].result.billitems.length > 0) {

                            var billitem = $scope.ngDialogData[0].result.billitems.find(function (item) {
                                return item.qty < 0;
                            });

                            if (typeof (billitem) != 'undefined' && billitem != null) {
                                $scope.Billitemid = billitem.id;
                            }
                        }


                    }, 100);


                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {

                    }
                }
            });
        }

        $scope.MultiPriceSerialno = function (value) {
            var request = {
                "Item": { 'id': $scope.result.stock.id, ismultiprice: value },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StockService/ChangeMultiplePrice",
                data: srequest
            }).success(function (result) {
                if ($scope.result != 'undefined' && $scope.result != null)
                    $scope.result.stock.ismultiprice = value;
            }).error(function (result) {
            });
        };

        $scope.PrintBarcode = function () {
            var ids = [];

            if ($scope.result.stock.barcode == 'undefined' || $scope.result.stock.barcode == '')
                return;
            $rootScope.Prompt('Print Barcode', 'Enter No of Copy', function (value) {
                ids.push({
                    'barcode': $scope.result.stock.barcode,
                    'noofcopy': parseInt(value)
                });
                $scope.ChoseReport({ 'iswarehouse': ($rootScope.LocationInfo.location.id == 0 ? true : false), 'locationid': $rootScope.LocationInfo.location.id, 'barcodes': ids, 'reportid': 0 });
            });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;
        }


        $scope.printDiv = function (div, title) {

            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = " Stock Report for ";

            var docHead = document.head.outerHTML;
            var printContents = document.getElementById(div).outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();

            angular.element('#apptitle').context.title = otitle;
        }


        $timeout(function () {
            $scope.LoadApplicationSetting();
            $scope.setFocusfirst();
        }, 10);

    }]);