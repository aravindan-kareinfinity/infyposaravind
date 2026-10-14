
var app = angular.module('InfinityLab');

app.controller('POInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService',
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
        $scope.Suppliers = restService.Get('ReferencelistService/LIST-SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': 0, 'name': 'All Locations' });
        });


        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });

        $scope.SearchRequest = {
            searchtype: 0,
            locationid: 0,
            statusid: 1
        };

        $scope.SearchType = [
            { 'id': 0, 'name': 'Default' },
            { 'id': 1, 'name': 'Supplier' },
            { 'id': 3, 'name': 'City' },
            { 'id': 4, 'name': 'Product' },
            { 'id': 5, 'name': 'Design' }];

        $scope.StatusList = [
            { 'id': 10, 'name': 'Pending' },
            { 'id': 11, 'name': 'Unapproved' },
            { 'id': 0, 'name': 'All' },
            { 'id': 1, 'name': 'Approved' },
            { 'id': 2, 'name': 'Rejected' },
            { 'id': 3, 'name': 'Completed' },
            { 'id': 6, 'name': 'GRN' },
            { 'id': 7, 'name': 'Billed' },
            { 'id': 8, 'name': 'Scanned' },
            { 'id': 9, 'name': 'Warehouse' }];

        $scope.SOList = [];

        $scope.SearchPOSummary = function () {
            $scope.totalrecords = 0;
            $scope.SOList = [];
            $scope.SearchRequest.potype = -5;
            request = {
                "item": $scope.SearchRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPOSummary",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.SOList = result;
                $scope.ShowList(result);
            }).error(function (result) {
            });
        }

        $scope.SelecteOrder = null;

        $scope.ShowList = function (result) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            $scope.selectedcount = 0;
            $scope.SelecteOrder = null;
            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('pono', 'Order No', 100);
            igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expirydate', 'Expiry Date', 100, 'dd-MM-yyyy');

            var column = igrid.AddCustomColumn('invoice', 'Invoice', 150, function (item, col) {
                if (item.invoiceno != null &&
                    item.invoiceno != '') {
                    return item.invoiceno + ' / ' + $filter('date')(item.invoicedate, 'dd-MM-yyyy');
                }
                return '';
            });
            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (item.invoiceno != null &&
                    item.invoiceno != '') {
                    return item.invoiceno + ' / ' + $filter('date')(item.invoicedate, 'dd-MM-yyyy');
                }
                return '';
            }
            igrid.AddSimpleColumn('suppliername', 'Supplier', 100);
            igrid.AddSimpleColumn('suppliercity', 'From Location', 100);
            igrid.AddSimpleColumn('remarks', 'Remarks', 100);
            igrid.AddReferenceColumn('internalstatus', 'Status', 100, $scope.StatusList);
            igrid.AddSimpleColumn('createdemployee', 'Created By', 100);
            var column = igrid.AddCustomColumn('actionemployee', 'Approval', 150, function (item, col) {
                if (item.status == 2)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status != 0 && item.status != 11)
                    return 'Approved - ' + item.actionemployee;
                return '';
            });
            column.sortable = true;
            column.filtertype = 'reference';
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (item.status == 2)
                    return 'Rejected - ' + item.actionemployee;
                if (item.status > 0)
                    return 'Approved - ' + item.actionemployee;
                return '';
            }

            igrid.AddReferenceColumn('deliverylocationid', 'To Location', 100, $scope.Location);
            igrid.AddFormatedColumn('ordered', 'Ordered', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('received', 'Billed', 80, 'N0', 2).total = true;
            igrid.AddButtonColumn('PDF', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PDF' class='btn btn-success btn-xs'><i class='fa fa-file-pdf-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'PDF') {
                    $scope.$apply(function () {
                        $rootScope.Download('PurchaseorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'po.pdf', 'application/pdf');
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

        $scope.ClonePO = function () {
            $scope.SelecteOrder = $scope.igrid.GetSelectedRows()[0];
            request = {
                "item": $scope.SelecteOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/ClonePO",
                data: srequest
            }).success(function (result) {
                
            }).error(function (result) {
            });
        }

        $scope.ChangeStatus = function (internalstatus) {
            $scope.SelecteOrder.internalstatus = internalstatus;
            if (internalstatus == 1 || internalstatus == 2)
                $scope.SelecteOrder.status = internalstatus;
            request = {
                "item": $scope.SelecteOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/UpdateStatus",
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

                        $scope.RemovePOGRN = function () {

                            var request = {
                                "Item": $scope.LREntry,
                                "SecurityToken": "String content"
                            }
                            var srequest = JSON.stringify(request);
                            $http({
                                method: 'POST',
                                url: "PurchaseorderService/RemovePOGRN",
                                data: srequest
                            }).success(function (result) {

                            });
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
                            var sorder = $scope.SelecteOrder;
                            $scope.SelecteOrder = null;

                            $timeout(function () {
                                sorder.internalstatus = data.value.internalstatus;
                                sorder.status = data.value.status;
                                if (sorder.internalstatus == undefined ||
                                    sorder.internalstatus == null ||
                                    sorder.internalstatus < 1) {
                                    sorder.internalstatus = 1;
                                    sorder.status = 1;
                                }
                                $scope.SelecteOrder = sorder;
                            }, 10);

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
            var subtitle = 'SALES ORDER - INFORMATION'
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
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.ShowPONO = $scope.OrderInfo.postatus.ShowPONO;
                    $scope.Orders = $scope.OrderInfo.postatus.orders;
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

                        $scope.UANPoList.postatus.orders = data.value;
                        var request = {
                            "item": $scope.UANPoList.postatus,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "PurchaseorderService/SaveUANPO",
                            data: srequest
                        }).success(function (result) {
                            $.alert("Total imported Orders - " + result.length);
                            $.each(result, function (k, v) {
                                $scope.SOList.splice(0, 0, v.order);
                            });
                            $scope.igrid.UpdateData($scope.SOList, '_id');
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
            $scope.SearchRequest.internalstatus = 10;
            $scope.SearchPOSummary();
        }
        $scope.LoadDefault();
    }]);
