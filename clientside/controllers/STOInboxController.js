
var app = angular.module('InfinityLab');

app.controller('STOInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
    'ngDialog', '$state', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];
        $scope.ismfgapplication = false;
        $scope.TransportModes = [{ 'id': 1, 'name': 'Road' }, { 'id': 2, 'name': 'Rail' }, { 'id': 3, 'name': 'Air' }, { 'id': 4, 'name': 'Ship' }];

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });


        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Locations' });
            if ($scope.SOList != null) {
                $scope.ShowList();
            }
        });

        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
            $scope.City = results;
        });

        restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Transports.push(v);
            })
        });

        $scope.gettransportname = function (id) {
            try {
                var found = $scope.Transports.find(function (arritem) {
                    return arritem.id == id;
                });
                if (typeof (found) != 'undefined' && found != null)
                    return found.name;
            } catch (e) {
                //alert(e);
            }
        }

        $scope.SearchRequest = {
            searchtype: 0,
            locationid: 0,
            statusid: 1
        };

        $scope.StatusList = [
            { 'id': -1, 'name': 'Requested' },
            { 'id': 0, 'name': 'Pending' },
            { 'id': 1, 'name': 'Inwarded' },
            { 'id': 2, 'name': 'Inwarded with Changes' },
            { 'id': 3, 'name': 'Pending Inward Bills' },
            { 'id': 9, 'name': 'Rejected' },
            { 'id': 10, 'name': 'All' }];

        $scope.SOList = null;

        $scope.SearchPOSummary = function () {
            $scope.totalrecords = 0;
            $scope.SOList = null;
            $scope.SearchRequest.bystatusid = true;
            $scope.SearchRequest.validatetolocation = true;
            request = {
                "item": $scope.SearchRequest,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/Search",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SOList = result;
                $scope.ShowList();
            }).error(function (result) {
            });
        }

        $scope.SelecteOrder = null;

        $scope.ShowList = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            $scope.selectedcount = 0;
            $scope.SelecteOrder = null;
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Order No', 100);
            igrid.AddFormatedColumn('packedon', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 100, $scope.Location);
            igrid.AddReferenceColumn('tolocationid', 'To Location', 100, $scope.Location);
            igrid.AddReferenceColumn('transportid', 'Transport', 100, $scope.Transports);
            igrid.AddFormatedColumn('itemsvalue', 'Amount', 80, 'N2', 2).total = true;

            igrid.AddSimpleColumn('notes', 'Remarks', 120);
            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            igrid.AddSimpleColumn('lrentryno', 'Inward No', 100);
            igrid.AddSimpleColumn('ewb', 'Eway', 100);
            igrid.AddSimpleColumn('einv', 'Ack', 100);
            igrid.AddSimpleColumn('createdbyemployee', 'Created By', 100);
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.StatusList);

            var column = igrid.AddCustomColumn('actionemployee', 'Approval', 150, function (item, col) {
                if (item.status == -3)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status == -2 || item.status >= 0)
                    return 'Approved - ' + item.actionemployee;
                return '';
            });

            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (item.status == -3)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status == -2 || item.status>=0)
                    return 'Approved - ' + item.actionemployee;
                return '';
            }

            igrid.AddFormatedColumn('qty', 'Qty', 80, 'N0', 2).total = true;
            igrid.AddButtonColumn('PDF', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PDF' class='btn btn-success btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('UNPACKEDPDF', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PDF' class='btn btn-success btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'PDF') {
                    $scope.$apply(function () {
                        $rootScope.Download('BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'sto.pdf', 'application/pdf');
                    })
                }
                if (command == 'UNPACKEDPDF') {
                    $scope.$apply(function () {
                        $rootScope.Download('BundlesService/PrintUnPackedPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'stounpacked.pdf', 'application/pdf');
                    })
                }
            });

            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SOList, '_id');
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    if (range.length == 1) {
                        $scope.SelecteOrder = $scope.igrid.GetSelectedRows()[0];
                    }
                    else {
                        $scope.SelecteOrder = null;
                    }
                }, 10);
            };
        }

        $scope.ChangeStatus = function (status) {
            $scope.SelecteOrder.status = status;
            request = {
                "item": $scope.SelecteOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $scope.SelecteOrder.actionby = result.actionby;
                $scope.SelecteOrder.actionon = result.actionon;
                $scope.SelecteOrder.actionemployee = result.actionemployee;
                $scope.igrid.UpdateData($scope.SOList, '_id');
                $scope.igrid.grid.setSelectedRows([]);
            }).error(function (result) {
            });
        }

        $scope.ShowGRN = function () {

            var request = {
                "Item": { 'importreferenceid': $scope.SelecteOrder.id, 'importreferencetype': 3 },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrentryService/Search",
                data: srequest
            }).success(function (result) {


                var dialog = ngDialog.open({
                    template: 'GRN',
                    data: {
                        'SelecteOrder': $scope.SelecteOrder,
                        'ParentScope': $scope,
                        'LREntry': result.length > 0 ? result[0] : { 'id': 0 }
                    },
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.ModeofDelivery = [{ 'id': 1, 'name': 'Lorry' }, { 'id': 2, 'name': 'Hand Delivery' },
                        { 'id': 3, 'name': 'Tailoring' }, { 'id': 4, 'name': 'Delivery Challan' },
                        { 'id': 5, 'name': 'Temporary' }, { 'id': 6, 'name': 'MFG - Jobwork' },
                        { 'id': 7, 'name': 'MFG - BOM' }];
                        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
                            $scope.Transports = results;
                        });
                        $scope.SelecteOrder = $scope.ngDialogData.SelecteOrder;
                        $scope.ParentScope = $scope.ngDialogData.ParentScope;
                        $scope.LREntry = $scope.ngDialogData.LREntry;
                        if ($scope.LREntry.id == 0) {
                            $scope.LREntry.locationid = $scope.SelecteOrder.deliverylocationid;
                            $scope.LREntry.supplierid = $scope.SelecteOrder.supplierid;
                            $scope.LREntry.companyid = $scope.SelecteOrder.companyid;
                            $scope.LREntry.lrcompanyid = $scope.SelecteOrder.lrcompanyid;
                            $scope.LREntry.importreferenceid = $scope.SelecteOrder.id;
                            $scope.LREntry.importreferencetype = 3;
                            $scope.LREntry.isactive = true;
                        }

                        $scope.Validate = function () {
                            if ($scope.LREntry.lrno == null || $scope.LREntry.lrno == 0) {
                                $rootScope.Toaster.Info("Please select LR Number.");
                                return false;
                            }

                            if ($scope.LREntry.receiveddate == null || $scope.LREntry.receiveddate == 0) {
                                $rootScope.Toaster.Info("Please select Received Date");
                                return false;
                            }

                            if ($scope.LREntry.lrdate == null || $scope.LREntry.lrdate == 0) {
                                $rootScope.Toaster.Info("Please select LR Date");
                                return false;
                            }

                            if ($scope.LREntry.invoicedate == null || $scope.LREntry.invoicedate == 0) {
                                $rootScope.Toaster.Info("Please select Invoice Date");
                                return false;
                            }

                            if ($scope.LREntry.totalamount == null || $scope.LREntry.totalamount == 0) {
                                $rootScope.Toaster.Info("Please enter value of bill.");
                                return false;
                            }

                            if ($scope.LREntry.invoiceno == null || $scope.LREntry.invoiceno == 0) {
                                $rootScope.Toaster.Info("Please select Invoice Number.");
                                return false;
                            }

                            if (($scope.LREntry.noofbundels == null || $scope.LREntry.noofbundels == '' || $scope.LREntry.noofbundels == 0)
                                && ($scope.LREntry.noofboxes == null || $scope.LREntry.noofboxes == '' || $scope.LREntry.noofboxes == 0)
                                && ($scope.LREntry.noofpieces == null || $scope.LREntry.noofpieces == '' || $scope.LREntry.noofpieces == 0)) {
                                $rootScope.Toaster.Info("Please enter either No of Bundles/Box/Pieces.");
                                return false;
                            }
                            if ($scope.LREntry.modeofdelivery == null || $scope.LREntry.modeofdelivery == 0) {
                                $rootScope.Toaster.Info("Please select mode of delivery");
                                return false;
                            }
                            return true;
                        }

                        $scope.Save = function () {
                            if (!$scope.Validate()) return;

                            var request = {
                                "Item": $scope.LREntry,
                                "SecurityToken": "String content"
                            }
                            $scope.SaveRequest = true;
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "PurchaseorderService/SavePOGRN",
                                data: srequest
                            }).success(function (result) {
                                $.alert("GRN Created for PO - " + result.pono);
                                $scope.closeThisDialog(result);
                            })
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                            && data.value != '$document') {
                            if (data.value.selected.length > 0) {

                            }
                        }
                    }
                });

            });
        }

        $scope.Print = function () {

            var rows = this.igrid.GetSelectedRows();
            if (rows.length == 0) {
                rows = $scope.SOList;
            }
            var printrows = [];
            $.each(rows, function (k, v) {
                if (v.plannedqty > 0) {
                    v.cityname = $scope.SearchRef(v.cityid, $scope.CityList, '');
                    v.productname = $scope.SearchRef(v.productid, $scope.ProductList, '');
                    var customer = $scope.SearchRef(v.customerid, $scope.Customers, '');
                    if (v.shipto && v.shipto > 0) {
                        customer += ' - ' + $scope.SearchRef(v.shipto, $scope.Customers, '');
                    }
                    v.customername = customer;
                    printrows.push(v);
                }
            });

            var sortBy = [{
                prop: 'customername',
                direction: 1
            },
            {
                prop: 'sono',
                direction: 1
            }

                , {
                prop: 'productname',
                direction: 1
            }, {
                prop: 'designitemcode',
                direction: 1
            }];

            printrows.sort(function (a, b) {
                var i = 0, result = 0;
                while (i < sortBy.length && result === 0) {
                    result = sortBy[i].direction * (a[sortBy[i].prop].toString() < b[sortBy[i].prop].toString() ? -1 : (a[sortBy[i].prop].toString() > b[sortBy[i].prop].toString() ? 1 : 0));
                    i++;
                }
                return result;
            });

            var list = [];

            list.push($rootScope.PrintFormater.Bold($rootScope.UserInfo.CompanyName));
            list.push($rootScope.PrintFormater.Bold("Delivery Plan ") + $filter('date')(new Date(), 'dd-MM-yyyy'));
            list.push($rootScope.PrintFormater.Normal("__________________________________________"));
            var customername = ''
            var cityname = '';
            var customerid = 0;
            $.each(printrows, function (k, v) {
                if (v.cityname != cityname) {
                    if (cityname != '') {
                        list.push("\r\n");
                        list.push($rootScope.PrintFormater.Cutter());
                    }
                    cityname = v.cityname;
                    list.push($rootScope.PrintFormater.Bold(cityname));
                }
                if (v.customername != customername) {
                    if (customerid > 0 && customerid == v.customerid) {
                        list.push($rootScope.PrintFormater.Barcode('SOPLAN/' + customerid, true));
                    }
                    customername = v.customername;
                    customerid = v.customerid;
                    list.push($rootScope.PrintFormater.Bold(customername));
                }
                var clist = [];
                clist.push($rootScope.PrintFormater.Normal(v.productname, 10, false));
                clist.push($rootScope.PrintFormater.Normal(v.designid, 8));
                clist.push($rootScope.PrintFormater.Normal(v.sizegroup, 8));
                clist.push($rootScope.PrintFormater.Normal(v.colourname, 8));
                clist.push($rootScope.PrintFormater.Align(v.plannedqty, 4, true));
                list.push(clist.join(' '));
            })

            var barcodevalue = $rootScope.PrintFormater.Barcode('SOPLAN/' + customerid);
            var base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); }
            var data = base64(barcodevalue);
            list.push($rootScope.PrintFormater.Barcode('SOPLAN/' + customerid));

            list.push("\r\n");
            list.push("\r\n");
            list.push($rootScope.PrintFormater.Cutter());
            list.push("\r\n");
            list.push("\r\n");

            var printdocument = window.btoa(list.join('\r\n'));
            $rootScope.Sent2Printer(printdocument);
        }

        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };


        $scope.CompleteSO = function () {

            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }
            var request = {
                "items": rows,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/CompleteSO",
                data: srequest
            }).success(function (result) {
                $.each(rows, function (k, v) {
                    v.pendingqty = 0;
                });
                $scope.igrid.UpdateData($scope.SOList, '_id');
            }).error(function (result) {
            });
        }
        $scope.CancelSO = function () {

            var rows = []
            if ($scope.Mode == 'Summary') {
                $.each(this.igrid.GetSelectedRows(), function (k, v) {
                    if (v.list && v.list.length > 0) {
                        $.each(v.list, function (kk, vv) {
                            rows.push(vv);
                        });
                    }
                });
            }
            else {
                rows = this.igrid.GetSelectedRows();
            }
            var request = {
                "items": rows,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/CancelSO",
                data: srequest
            }).success(function (result) {
                $.each(rows, function (k, v) {
                    v.pendingqty = 0;
                });
                $scope.igrid.UpdateData($scope.SOList, '_id');
            }).error(function (result) {
            });
        }


        $scope.ExportPivot = function () {


            var worksheetName = 'Quanto Report';

            var processRow = function (row) {
                var finalVal = '';
                for (var j = 0; j < row.length; j++) {
                    var innerValue = row[j] === null ? '' : row[j].toString();
                    if (row[j] instanceof Date) {
                        innerValue = row[j].toLocaleString();
                    };
                    var result = innerValue.replace(/"/g, '""');
                    if (result.search(/("|,|;|\n)/g) >= 0)
                        result = '"' + result + '"';
                    if (j > 0)
                        finalVal += ',';
                    finalVal += result;
                }
                return finalVal + '\n';
            };

            var csvFile = '';
            var rows = [];
            rows.push('<Thead>')
            var colname = [];
            var skipIndex = [];
            var formator = [];

            var colcount = 0;
            var columns = [
                { title: 'City', field: 'cityname', 'group': true, 'last': '' },
                { title: 'Customer', field: 'customername', 'group': true, 'last': '' },
                { title: 'Product', field: 'productname', 'group': true, 'last': '' },
                { title: 'Design', field: 'designid', 'group': false, 'last': '' },
                { title: 'Colour', field: 'colourname', 'group': false, 'last': '' },
                { title: 'Size/Set', field: 'sizegroup', 'group': false, 'last': '' },
                { title: 'Pending', field: 'pendingqty', 'group': false, 'last': '' },
                { title: 'Available', field: 'availableqty', 'group': false, 'last': '' }
            ];
            var sortingfields = {
                cityname: true, customername: true, customername: true
                , productname: true, designid: true, colourname: true
                , sizegroup: true
            } // This is used as a dictionary. Sort by "name" DESC and "age" ASC

            var datarows = this.igrid.GetSelectedRows();
            if (datarows.length == 0)
                datarows = $scope.SOList;

            datarows.sort((a, b) => {
                for (col in sortingfields) { // added properties are positional
                    if (a[col] != b[col]) { // if they are equal, sorting is made on the next column
                        if (sortingfields[col] === true) return a[col] > b[col] // if ASC === true
                        else return a[col] < b[col]
                    }
                }
            })


            rows.push('<Tr>')
            for (var j = 0, len = columns.length; j < len; j++) {
                colname.push('<th>');
                colname.push(columns[j].title);
                colname.push('</th>');
            }
            rows.push(colname.join(''));
            rows.push('</Tr></Thead><Tbody>')

            var singlerow = [];
            for (var i = 0, l = datarows.length; i < l; i++) {
                for (var j = 0, len = columns.length; j < len; j++) {
                    singlerow.push('<td>');
                    var item = datarows[i];
                    var column = columns[j];
                    var value = item[column.field];
                    if (value == null) value = '';
                    if (column.group) {
                        if (column.last == value) {
                            singlerow.push('');
                        }
                        else {
                            singlerow.push(value);
                            column.last = value;
                        }
                    }
                    else {
                        singlerow.push(value);
                    }
                    singlerow.push('</td>');
                }
                rows.push('<tr>');
                rows.push(singlerow.join(''));
                rows.push('</tr>');
                singlerow = [];
            }

            rows.push('</Tbody>')


            tableContent = rows.join('');

            var uri = 'data:application/vnd.ms-excel;base64,',
                template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table border="1">{table}</table></body></html>',
                base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))); },
                format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) };
            var ctx = { worksheet: worksheetName, table: tableContent };


            var blob = new Blob([format(template, ctx)], { type: 'application/vnd.ms-excel;charset=utf-8;' });
            if (navigator.msSaveBlob) { // IE 10+
                navigator.msSaveBlob(blob, worksheetName + ".xls");
                return null;
            } else {
                var link = document.createElement("a");
                if (link.download !== undefined) { // feature detection
                    // Browsers that support HTML5 download attribute
                    var url = URL.createObjectURL(blob);
                    link.setAttribute("href", url);
                    link.setAttribute("download", worksheetName + ".xls");
                    link.style.visibility = 'hidden';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    return null;
                }
            }

            href = uri + base64(format(template, ctx));

            var link = document.createElement("a");
            link.setAttribute("href", href);
            link.setAttribute("download", "filename.csv");
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'STOCK ORDER - INFORMATION'
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }


        $scope.uanpofileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            $scope.UANPoList = value;
            var dialog = ngDialog.open({
                template: 'ImportPO',
                data: {
                    'OrderInfo': value,
                    'ParentScope': $scope
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.stockfailed = $scope.OrderInfo.stostatus.stocklist && $scope.OrderInfo.stostatus.stocklist.length > 0;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Orders = $scope.OrderInfo.stostatus.orders;
                    $scope.ShowBundleCode = $scope.OrderInfo.stostatus.ShowBundleCode;
                    $scope.Count = 0;
                    $scope.SelectedAll = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            if (v.valid) {
                                v.selected = $scope.selectedall;
                                if ($scope.selectedall)
                                    $scope.Count += 1;
                            }
                        });
                    }
                    $scope.SelectedChanged = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            $scope.Count += (typeof (v.selected) != 'undefined' && v.selected) ? 1 : 0;
                        });
                    }

                    $scope.MissingStock = function () {
                        var data = [];
                        data.push([
                            'UAN',
                            'STOCK',
                            'SO',
                            'STO',
                            'POSSIBLE',
                            'REQUIRED'
                        ]);

                        var stock = 0;
                        var required = 0;
                        var so = 0;
                        var sto = 0;
                        var possible = 0;
                        $.each($scope.OrderInfo.stostatus.stocklist, function (k, v) {
                            data.push([
                                v.uan,
                                v.stock,
                                v.so,
                                v.sto,
                                (v.stock - (v.so + v.sto)),
                                v.required
                            ]);
                            stock += v.stock;
                            so += v.so;
                            possible += (v.stock - (v.so + v.sto));
                            sto += v.sto;
                            required += v.required;
                        });

                        data.push([
                            'Total',
                            stock,
                            so,
                            sto,
                            possible,
                            required,
                            '',
                        ]);

                        if (data.length > 0) {

                            var title = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "center"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };

                            var Stitle = {									// set the style for target cell
                                font: {
                                    name: 'verdana',
                                    sz: 11,
                                    bold: true,
                                    color: { rgb: "33220000" }
                                },
                                alignment: {
                                    vertical: "center",
                                    horizontal: "right"
                                },
                                border: {
                                    top: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    bottom: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                }
                            };
                            var boderstyle = {
                                border: {
                                    left: {
                                        style: 'thin', color: { rgb: "000000" }
                                    },
                                    right: {
                                        style: 'thin', color: { rgb: "000000" }
                                    }
                                },
                            }

                            var ws = XLSX.utils.aoa_to_sheet(data);
                            var wb = XLSX.utils.book_new();

                            ws["A1"].s = title;
                            ws["B1"].s = title;
                            ws["C1"].s = title;
                            ws["D1"].s = title;
                            ws["E1"].s = title;
                            ws["F1"].s = title;

                            ws["A" + data.length].s = Stitle;
                            ws["B" + data.length].s = Stitle;
                            ws["C" + data.length].s = Stitle;
                            ws["D" + data.length].s = Stitle;
                            ws["E" + data.length].s = Stitle;
                            ws["F" + data.length].s = Stitle;

                            ws['!cols'] = [{ width: 20 }, { width: 10 }, { width: 10 }, { width: 10 }, { width: 15 }, { width: 15 }];

                            var prefix = '';
                            var startfrom = 64;
                            for (var ridx = 1; ridx <= 6; ridx++) {
                                startfrom++;
                                for (var rnidx = 2; rnidx < data.length; rnidx++) {
                                    ws[prefix + String.fromCharCode(startfrom) + rnidx].s = boderstyle;
                                }
                                if (startfrom == 90) {
                                    charcode = 65;
                                    if (prefix != '') {
                                        var charcode = prefix.charCodeAt(prefix.length - 1);
                                        charcode++;
                                        if (charcode > 90) {
                                            charcode = 65;
                                        }
                                    }
                                    prefix = String.fromCharCode(charcode);
                                    startfrom = 64;
                                }
                            }

                            XLSX.utils.book_append_sheet(wb, ws, "Stock");
                            const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
                            saveAs(new Blob([wbout], { type: "application/octet-stream" }), "stockshortage.xlsx");
                        }
                    }

                    $scope.Save = function () {
                        var selected = [];
                        var skipped = [];
                        $.each($scope.Orders, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected) {
                                if (v.valid) {
                                    selected.push(v);
                                }
                                else {
                                    skipped.push(v);
                                }
                            }
                        });
                        if (skipped.length > 0) {
                            $rootScope.Toaster.Info("Invalid orders can't import");
                        }
                        else {
                            $scope.closeThisDialog(selected);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {

                        $scope.UANPoList.stostatus.orders = data.value;
                        var request = {
                            "item": $scope.UANPoList.stostatus,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BundlesService/SaveUANSTO",
                            data: srequest
                        }).success(function (result) {
                            $.alert("Total imported Orders - " + result.length);
                        });

                    }
                }
            });


        }

        $scope.StatusChange = function (action) {

            var items = $scope.GetSelecteItemsstatus();
            if (items.length == 0)
                return;

            var inboxitems = [];
            $.each(items, function (k, v) {
                var inboxstatus = { 'id': v.id, 'hasclosed': false };
                inboxstatus.statusname = action;

                if (action == "Working") {
                    inboxstatus.statusid = 1;
                }
                else if (action == "Completed") {
                    inboxstatus.statusid = 2;
                    inboxstatus.hasclosed = true;
                }
                else if (action == "Cancel") {
                    inboxstatus.statusid = 3;
                }
                inboxitems.push(inboxstatus);
            });

            request = {
                "Items": inboxitems,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "InboxService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $.each(items, function (k, v) {
                    $scope.LRInboxList.splice($scope.LRInboxList.indexOf(v), 1);
                });
            }).error(function (result) {
            });
        };

        $scope.ShowBilling = function () {
            $location.url('/PurchaseOrderGRN?importreferencetype=3&internalstatus=7&importreferenceid=' + $scope.SelecteOrder.id);
        }
        $scope.ShowScanning = function () {
            $location.url('/PurchaseOrderGRN?importreferencetype=3&internalstatus=8&importreferenceid=' + $scope.SelecteOrder.id);
        }

        $scope.Inward = function (bundle) {
            $location.url('/Inward?id=' + bundle.id);
        }

        $scope.ReceiveGoods = function (bundle) {
            $location.url('/ReceiveGoods?id=' + bundle.id);
        }

        $scope.ScanBarcodes = function (bundle,addtionalqs) {
            $location.url('/STOInboxScan?id=' + bundle.id + addtionalqs);
        }

        $scope.CreateBill = function (bundle) {
            var request = {
                "item": { 'id': bundle.id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/CreateBill",
                data: srequest
            }).success(function (result) {
                $.alert("New Bill Created - " + result.billno);
            });
        }

        $scope.ManageLR = function (item, callback) {
            var request = {
                "item": { 'billid': item.billid,'status':1 },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BillinboxService/Search",
                data: srequest
            }).success(function (result) {
                if (result && result.length == 1) {
                    $scope.ShowManageLRDialog(result[0]);
                }
            });
        }

        $scope.ShowManageLRDialog = function (item, callback) {
            var dialog = ngDialog.open({
                template: 'ManageLR',
                data: [$scope, item],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', 'restService', '$rootScope', function ($scope, $timeout, $http, restService, $rootScope) {
                    $scope.entity = $scope.ngDialogData[1];
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                    $scope.editeinvoice = false;
                    $scope.editeway = false;
                    if (typeof ($scope.entity.billimg) != 'undefined' &&
                        $scope.entity.billimg != null && $scope.entity.billimg != '') {
                        $scope.imagelist = $scope.entity.billimg.split(',');
                    }
                    else {
                        $scope.imagelist = [];
                    }
                    if ($scope.entity.lrdate == 'undefined' || $scope.entity.lrdate == 'null'
                        || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                        $scope.entity.lrdate = new Date();
                    }

                    $scope.publish = function () {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/Publish",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog();
                        });
                    }

                    $scope.TransportChanged = function () {
                        var transport = $scope.Parent.Transports.find(function (item) {
                            return item.id == $scope.entity.transportid;
                        });
                        if (transport && transport != null) {
                            $scope.entity.vehicleno = transport.vehicles.split(',')[0];
                        }
                    }

                    $scope.billimg = null;
                    $scope.GenerateEInvoice = function (mode) {
                        $scope.entity.transportname = $scope.Parent.gettransportname($scope.entity.transportid);
                        $scope.entity.mode = mode
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/GenerateEInvoice",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.CancelEway = function (mode) {
                        $scope.Question('Eway Cancellation', 'Are you sure, do you want to cancel the eway for this bill', function () {
                            $scope.InternalCancelEway(mode);
                        })
                    }

                    $scope.InternalCancelEway = function (mode) {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/CancelEway",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.Question = function (title, content, callback) {
                        $.confirm({
                            title: title,
                            content: content,
                            confirmButton: 'Yes',
                            confirmButtonClass: 'btn-info',
                            icon: 'fa fa-question-circle',
                            animation: 'scale',
                            animationClose: 'top',
                            opacity: 0.5,
                            buttons: {
                                formSubmit: {
                                    text: 'Submit',
                                    btnClass: 'btn-blue',
                                    action: function () {
                                        callback();
                                    }
                                },
                                cancel: function () {
                                    //close
                                },
                            },

                            onContentReady: function () {
                                // bind to events
                                var jc = this;
                                jc.$$formSubmit.trigger('focus');
                                this.$content.find('form').on('submit', function (e) {
                                    // if the user submits the form by pressing enter in the field.
                                    e.preventDefault();
                                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                                });
                            }
                        });
                    }

                    $scope.CancelEInvoice = function (mode) {
                        $scope.Question('Einvoice Cancellation', 'Are you sure, do you want to cancel the e-invoice for this bill', function () {
                            $scope.InternalCancelEInvoice(mode);
                        })
                    }


                    $scope.InternalCancelEInvoice = function (mode) {
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/CancelEInvoice",
                            data: srequest
                        }).success(function (result) {

                            if (result.ErrorDetails && result.ErrorDetails.length > 0) {
                                var errors = [];
                                $.each(result.ErrorDetails, function (k, v) {
                                    errors.push(v.ErrorMessage);
                                });
                                $rootScope.ShowAlert('Error', errors.join('<br>'));
                            } else {
                                $scope.entity.version = result.BillInbox.version;
                                $scope.entity.einvoice = result.BillInbox.einvoice;
                                $scope.entity.ewb = result.BillInbox.ewb;
                                $scope.entity.modifiedby = result.BillInbox.modifiedby;
                                $scope.entity.modifiedon = result.BillInbox.modifiedon;
                            }

                        });
                    }

                    $scope.BillImageChanged = function (imageid) {
                        if (typeof (imageid) != 'undefined') {
                            if (angular.isArray(imageid)) {
                                if (imageid[0] != null && imageid[0] != '') {
                                    $scope.imagelist.splice(0, 0, imageid[0]);
                                }
                            }
                            else if (imageid != null && imageid != '') {
                                $scope.imagelist.splice(0, 0, imageid);
                            }
                        }
                        else {
                            $scope.imagelist.splice(0, 0, $scope.billimg);
                        }
                        $scope.billimg = null;
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    $scope.RemoveImage = function (item) {
                        $scope.imagelist.splice($scope.imagelist.indexOf(item), 1);
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }


                    $scope.Save = function () {
                        $scope.entity.modeofdelivery = 1;
                        $scope.entity.transportname = $scope.Parent.gettransportname($scope.entity.transportid);
                        request = {
                            "Item": $scope.entity,
                            "SecurityToken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BillinboxService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        callback(data.value);
                    }
                }
            });
        };

        $scope.DownloadBillPdf = function (item) {
            $rootScope.Download("BillService/PrintPDF/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false", 'billpdf-' + item.billid + '.pdf', 'application/pdf');
        }

        $scope.PrintDeliverySlip = function (item) {
            $rootScope.Download("BillService/PrintDeliverySlip/" + $scope.UserInfo.OrganizationID + '/' + item.billid + "/false", 'billpdf-' + item.billid + '.pdf', 'application/pdf');
        }


         $scope.ShowChangeStatus = function (item) {
            var dialog = ngDialog.open({
                template: 'ChooseStatus',
                data: [$scope, item],
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.entity = $scope.ngDialogData[1];
                    $scope.ChangeStatus = function () {
                        $scope.closeThisDialog($scope.newstatus);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' &&
                        data.value != '$document') {
                        $scope.ChangeStatus(data.value);
                    }
                }
            });
        }


        $scope.ShowTransport = function (item) {
            var dialog = ngDialog.open({
                template: 'Transport',
                data: [$scope, item],
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.entity = $scope.ngDialogData[1];

                    if (typeof ($scope.entity.billimg) != 'undefined' &&
                        $scope.entity.billimg != null && $scope.entity.billimg != '') {
                        $scope.imagelist = $scope.entity.billimg.split(',');
                    }
                    else {
                        $scope.imagelist = [];
                    }

                    if ($scope.entity.lrdate == undefined || $scope.entity.lrdate == null
                        || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                        $scope.entity.lrdate = new Date();
                    }

                    $scope.TransportModes = [{ 'id': 1, 'name': 'Road' }, { 'id': 2, 'name': 'Rail' }, { 'id': 3, 'name': 'Air' }, { 'id': 4, 'name': 'Ship' }];
                    $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
                        $scope.Transports = results;
                    });

                    $scope.BillImageChanged = function (imageid) {
                        if (typeof (imageid) != 'undefined') {
                            if (angular.isArray(imageid)) {
                                if (imageid[0] != null && imageid[0] != '') {
                                    $scope.imagelist.splice(0, 0, imageid[0]);
                                }
                            }
                            else if (imageid != null && imageid != '') {
                                $scope.imagelist.splice(0, 0, imageid);
                            }
                        }
                        else {
                            $scope.imagelist.splice(0, 0, $scope.billimg);
                        }
                        $scope.billimg = null;
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    $scope.RemoveImage = function (item) {
                        $scope.imagelist.splice($scope.imagelist.indexOf(item), 1);
                        $scope.entity.billimg = $scope.imagelist.join(',');
                    }

                    restService.Get('BundlesService/Transport/' + $scope.entity.id, true).then(function (results) {
                        if (results && results != null) {
                            $scope.entity = results;
                            if ($scope.entity.lrdate == undefined || $scope.entity.lrdate == null
                                || $scope.entity.lrdate == '0001-01-01T00:00:00') {
                                $scope.entity.lrdate = new Date();
                            }
                            if (typeof ($scope.entity.billimg) != 'undefined' &&
                                $scope.entity.billimg != null && $scope.entity.billimg != '') {
                                $scope.imagelist = $scope.entity.billimg.split(',');
                            }
                            else {
                                $scope.imagelist = [];
                            }
                        }
                    });

                    $scope.Selected = function () {
                        request = {
                            "Item": $scope.entity,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BundlesService/Transport",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $rootScope.Toaster.Info("Successfully Generated");
                                $scope.entity.ewb = results.ewb;
                                $scope.closeThisDialog(null);
                            }

                        }).error(function (result) {
                        });

                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    }
                }
            });
        }

        $scope.LoadDefault = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadDefault();
                }, 50);
                return;
            }
            var today = new Date();
            var beginfrom = new Date((today).setDate(today.getDate() - 14));

            $scope.SearchRequest.fromdate = $filter('date')(beginfrom, datePicker.shortDisplay);
            $scope.SearchRequest.todate = $filter('date')(new Date(), datePicker.shortDisplay);
            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            if ($scope.system_possizelist == null) $scope.system_possizelist = '';
            $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
            $scope.SearchRequest.statusid = 0;
            $scope.SearchPOSummary();
        }
        $scope.LoadDefault();
    }]);
