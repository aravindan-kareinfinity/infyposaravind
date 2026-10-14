
var app = angular.module('InfinityLab');

app.controller('GRNInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog','$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state) {
        $scope.SearchRef = restService.SearchReference;
        $scope.filteredtype = '';
        $scope.Transports = [];
        $scope.City = [];
        $scope.LRInboxList = [];
        $scope.Suppliers = [];
        $scope.Company = [];
        $scope.Location = [];

        /*LREntry,
            Courier,
            LRIssue,
            LRReceived,
            Package*/
        $scope.ItemTypes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 2, 'name': 'Courier' }, { 'id': 1, 'name': 'LR Entry' }, { 'id': 8, 'name': 'Lorry Delivery' }, { 'id': 7, 'name': 'Hand Delivery' },
            { 'id': 3, 'name': 'LR Issue' }, { 'id': 4, 'name': 'LR Received' }, { 'id': 9, 'name': 'Purchase Return' }, { 'id': 5, 'name': 'Package' }, { 'id': 6, 'name': 'Pending' }];

        /*
            Open=0,
            Working = 1,
            Completed = 2,

            New_LR=200,
            Invoice_Preparation=201,
            Items_Entering=202,
            Barcode_Generating=203,

            //Courier
            Received = 100,
            Dispatched = 101,
            Send = 102,
            Sent = 103,*/

        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Company.push(v);
            });
            $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
            $scope.ConfigGrid();
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Location.push(v);
            })
        });

        restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Transports.push(v);
            })
            $scope.PreLoad();
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

        $scope.StatusList = [{ 'id': 0, 'name': 'Open' }, { 'id': 1, 'name': 'Working' }, { 'id': 2, 'name': 'Completed' }, { 'id': 3, 'name': 'Cancelled' },
        { 'id': 100, 'name': 'Received' }, { 'id': 101, 'name': 'Dispatched' }, { 'id': 102, 'name': 'Send' }, { 'id': 103, 'name': 'Sent' },
        { 'id': 200, 'name': 'New LR' },
        { 'id': 201, 'name': 'LR Issue' }, { 'id': 202, 'name': 'LR Received' },
        { 'id': 203, 'name': 'Invoice Preparation' }, { 'id': 204, 'name': 'Items Entering' }, { 'id': 205, 'name': 'Barcode Generating' }];

        $scope.typechanged = function () {
            if ($scope.SearchRequest.typeid == 6 || $scope.SearchRequest.typeid == 7 || $scope.SearchRequest.typeid == 0) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.ItemTypes.find(function (item) {
                    return $scope.SearchRequest.typeid == item.id;
                });
                if (ftype != null && ftype.id > 0)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };


        $scope.supplierchanged = function () {

            var ftype = $scope.Suppliers.find(function (item) {
                return $scope.SearchRequest.supplierid == item.id;
            });
            if (ftype != null && ftype.id > 0)
                $scope.filteredtype = ftype.name;
            else
                $scope.filteredtype = '';

        };

        $scope.statuschanged = function () {
            if ($scope.SearchRequest.statusid < 200) {
                $scope.filteredtype = '';
            }
            else {
                var ftype = $scope.StatusList.find(function (item) {
                    return $scope.SearchRequest.statusid == item.id;
                });
                if (ftype != null && ftype.id >= 200)
                    $scope.filteredtype = ftype.name;
                else
                    $scope.filteredtype = '';
            }
        };

        $scope.SearchRequest = {
            typeid: 0,
            addedfrom: null,
            supplierid: null,
            statusid: null,
            addedto: null
        };

        restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.Suppliers.push(v);
            })
        });
        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.City.push(v);
            })
        });

        $scope.Search = function () {
            $scope.LRInboxList = [];
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "InboxService/Search",
                data: srequest
            }).success(function (result) {
                //alert(result.length);
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }



        $scope.ManageLRInboxList = function (item) {
            if (item.typeid == 2) {
                $location.url('/Courier?id=' + item.referenceid);
            }
            else if (item.typeid == 1) {
                $location.url('/LREntry?id=' + item.referenceid)
            }
            else if (item.typeid == 3) {
                $location.url('/IssueLR?id=' + item.referenceid);
            }
            else if (item.typeid == 4) {
                $location.url('/ReceiptLR?id=' + item.referenceid);
            }
            else if (item.typeid == 7) {
                $location.url('/PurchaseReturn?id=' + item.referenceid);
            }
        };

        $scope.GetSelecteItemObject = function (type) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v);
            });
            return retList;
        };

        $scope.GetSelecteItems = function (type, statusid, modeofdelivery) {
            var seletedRows = this.igrid.GetSelectedRows();
            var retList = [];
            $.each(seletedRows, function (k, v) {
                if ((type == null || v.typeid == type) && (typeof (statusid) == 'undefined' || statusid == null || v.statusid == statusid) && (typeof (modeofdelivery) == 'undefined' || v.modeofdelivery == modeofdelivery))
                    retList.push(v.referenceid);
            });
            return retList;
        };

        $scope.GetSelecteItemsstatus = function () {

            var seletedRows = this.igrid.GetSelectedRows();

            var retList = [];
            $.each(seletedRows, function (k, v) {
                    retList.push(v);
            });
            return retList;
        };

        $scope.LRIssue = function () {
            var list = $scope.GetSelecteItems(1, 200, 1);
            if (list.length > 0) {
                $location.url('/IssueLR?ids=' + list.join());
            }
            else {
                $rootScope.ShowAlert('InBox', "Please select Lorry - LR, Which don't have LR Issues.");
            }
        };

        $scope.LRReceipt = function () {
            var list = $scope.GetSelecteItems(3);
            if (list.length == 1) {
                $location.url('/ReceiptLR?ids=' + list[0]);
            }
        };

        $scope.LRInvoice = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (list[0].modeofdelivery == 1 && (list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2))) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow invoice entry LR has issue & receive.");
                    return;
                }
                $location.url('/Invoice?lrid=' + list[0].referenceid);
            }
        };

        $scope.LRProducts = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't allow enter invoice items until LR has issue & receive.");
                    return;
                }
                else if (list[0].statusid < 203 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice before entering invoice items.");
                    return;
                }
                $location.url('/LRInvoiceItems?lrid=' + list[0].referenceid);
            }
        };

        $scope.ProductBarcode = function () {
            var list = $scope.GetSelecteItemObject(1);
            if (list.length == 1) {
                if (list[0].modeofdelivery == 1 && list[0].statusid < 202 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Lorry delivery can't generate barcode until LR has issue & receive.");
                    return;
                } else if (list[0].statusid < 204 && !(list[0].statusid == 1 || list[0].statusid == 2)) {
                    $rootScope.ShowAlert('InBox', "Pleae enter invoice items before generating barcodes.");
                    return;
                }
                $location.url('/barcode?lrid=' + list[0].referenceid);
            }
        };




        $scope.PreLoad = function () {
            request = {
                "Item": {
                    typeid: 0,
                    addedfrom: null,
                    addedto: null,
                    showallopen: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "InboxService/Search",
                data: srequest
            }).success(function (result) {
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WAREHOUSE DASHBOARD - INFORMATION'
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

        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true,0);
            igrid.options.showHeaderRow = true;
            igrid.AddCustomColumn('modeofdelivery', '', 40, function (item) {
                return "<img src='clientside/dist/img/delivery-" + item.modeofdelivery + ".png' />";
            });

            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('lrcompanyid', 'Company', 100, $scope.Company);
            if ($scope.ismultilocationentry)
                igrid.AddReferenceColumn('entrylocationid', 'Location', 100, $scope.Location);

            igrid.AddSimpleColumn('typedisplay', 'Type', 80);
            igrid.AddSimpleColumn('statusname', 'Status', 100);
            igrid.AddFormatedColumn('addedon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
            igrid.AddSimpleColumn('lrentrydetail', 'LR No', 100);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receiveddate', 'LR Recd. Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Inv.No', 75);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 100, 'dd-MM-yyyy');

            igrid.AddSimpleColumn('suppliername', 'Supplier', 200);
            igrid.AddReferenceColumn('cityid', 'Supplier City', 100, $scope.City);
            igrid.AddReferenceColumn('transportid', 'Transport', 140, $scope.Transports);
            igrid.AddFormatedColumn('noofbundels', 'No of Bundle', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('noofpieces', 'No of Pieces', 80, 'N2', 0).total = true;
            igrid.AddSimpleColumn('createdusername', 'Created By', 100);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Manage'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Validate'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Manage')
                        $scope.ManageLRInboxList(args.item);
                    else if ($(e.target).val() == 'Validate') {
                        $state.go('PurchaseInsight', {
                            source: 'lrinbox',
                            data: { 'lrid': args.item.referenceid }
                        });
                    }
                });
            });

            //igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
        }

    }]);
