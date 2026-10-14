var app = angular.module('InfinityLab');

app.controller('ReceiptLRController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.BookingOffice = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.BookingOffice = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.ReceivedBy = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.ReceivedBy = results;
        });

        $scope.FromLocations = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.FromLocations = results;
        });

        // model to be used to ReciveLR

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "receiptno": null,
            "bookingofficeid": 0,
            "receivedon": "",
            "receivedby": 0,
            "issueid": 0,
            "issueno": null,
            "issuedate": "",
            "frieghtcharges": 0.0,
            "noofbundels": 0.0,
            "noofpieces": 0.0,
            "noofboxes": 0.0,
            "actualweight": 0.0,
            "chargedweight": 0.0,
            "loadingcharges": 0.0,
            "receiptitem": [{
                "id": 0, "isactive": true, "createdby": 0, "modifiedby": 0, "version": 1, "createdon": "", "modifiedon": "", "lrid": 0, "lrno": "", "noofbundels": 0, "totalamount": 0, "isreversed": false, "lrreceivedid": 0, rweight: "",
                insurance: "0",
                packing: "0",
                forward: "0",
                others: "0",
                totalamount: "0"
            }],
            securityinwardno: "",
            securityinwarddate: "",
            lramount: ""
        };

        $scope.ReceiptLR = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.SearchCondition = {
            receiptno: "",
            receivedon: "",
            bookingofficeid: ""
        }

        $scope.computefright = function (item) {
            if (typeof (item.perkg) != 'undefined' && item.perkg > 0) {
                item.frightcharge = item.chargedweight * item.perkg;
                $scope.computecharge(item);
            }
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };


        $scope.computecharge = function (item) {
            $scope.ReceiptLR.chargedweight = 0;
            $scope.ReceiptLR.receivedweight = 0;
            $scope.ReceiptLR.frieghtcharges = 0;
            $scope.ReceiptLR.loadingcharges = 0;
            $scope.ReceiptLR.othercharges = 0;
            $.each($scope.ReceiptLR.receiptitem, function (k, v) {
                if (!v.isreversed) {
                    $scope.ReceiptLR.chargedweight += $scope.GetNumber(v.chargedweight);
                    $scope.ReceiptLR.receivedweight += $scope.GetNumber(v.receivedweight);
                    $scope.ReceiptLR.frieghtcharges += $scope.GetNumber(v.frightcharge);
                    $scope.ReceiptLR.loadingcharges += $scope.GetNumber(v.loadingcharge);
                    $scope.ReceiptLR.othercharges += $scope.GetNumber(v.insurance) +
                        $scope.GetNumber(v.packing) + $scope.GetNumber(v.forward) + $scope.GetNumber(v.others);
                }
            });
            $scope.ReceiptLR.totalcharges = $scope.ReceiptLR.frieghtcharges + $scope.ReceiptLR.loadingcharges + $scope.ReceiptLR.othercharges;
        }

        $scope.ManageLRItem = function (item) {
            var index = $scope.ReceiptLR.receiptitem.indexOf(item);
            $scope.ReceiptLR.receiptitem.splice(index, 1);
        }


        $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };

        $scope.SaveRequest = false;
        $scope.Save = function () {

            if ($scope.SaveRequest) return;

            if ($scope.ReceiptLR.receivedby == null || $scope.ReceiptLR.receivedby == 0) {
                $rootScope.Toaster.Error("Please select Received By");
                return;
            }

            if ($scope.ReceiptLR.receivedon == null || $scope.ReceiptLR.receivedon == 0) {
                $rootScope.Toaster.Error("Please Select Received On Date");
                return;
            }

            $scope.SaveRequest = true;
            request = {
                "Item": $scope.ReceiptLR,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrreceivedService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("ReceiptLR Saved Successfully.! Receipt No - (" + result.receiptno + ")");
                    $scope.ReceiptLR.receiptno = result.receiptno;
                    $scope.ReceiptLR = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });

        }


        $scope.ManageLRItem = function (action, item) {

        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete Receipt ' + item.receiptno + "?",
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

                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "LrreceivedService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                    }

                                }).error(function (result) {

                                });
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
            else if (action == "Edit") {
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.ReceiptLR = item;
                $scope.GetLrReceipt();
            }
        }
        $scope.GetLrReceipt = function () {

            request = {
                "Item": { id: $scope.ReceiptLR.id },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrreceivedService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.ReceiptLR = result[0];
                    $scope.igrid.UpdateData($scope.ReceiptLR.receiptitem, '_id');
                }
            }).error(function (result) {
            });

        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'LR RECEIPT REPORT');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "lrentrylist.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }


        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrreceivedService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.ReceiptConfig();
                $scope.sgrid.UpdateData($scope.SearchResult, '_id');
            }).error(function (result) {
            });

        }
        $scope.ReceiptConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('issueno', 'Issue No', 80);
            igrid.AddFormatedColumn('issuedate', 'Issue Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 100);
            igrid.AddFormatedColumn('receivedon', 'Received Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('receivedby', 'Received By', 150, $scope.ReceivedBy);
            igrid.AddSimpleColumn('lrno', 'LR No', 80);
            igrid.AddReferenceColumn('bookingofficeid', 'Booking Office', 200, $scope.BookingOffice);
            igrid.AddFormatedColumn('noofbundels', 'No of Bundles', 60, 'N2', 2);
            igrid.AddFormatedColumn('frieghtcharges', 'Fright', 60, 'N2', 2);

            var commands = "";
            if ($rootScope.UserInfo.p_LRRECEIPT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_LRRECEIPT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_LRRECEIPT.modify || $rootScope.UserInfo.p_LRRECEIPT.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }


        if (typeof ($location.search().ids) != 'undefined' &&
            $location.search().ids.length > 0) {
            request = {
                "Item": {
                    'id': $location.search().ids,
                    'computecharges': true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrissueService/Search",
                data: srequest
            }).success(function (result) {
                // alert(result.items.length);
                $scope.ReceiptLR.issueid = result[0].id;
                $scope.ReceiptLR.issueno = result[0].issueno;
                $scope.ReceiptLR.issuedate = result[0].issuedate;
                $scope.ReceiptLR.bookingofficeid = result[0].bookingofficeid;
                $scope.ReceiptLR.receivedby = result[0].collectingby;
                $scope.ReceiptLR.receiptitem = result[0].lrissuelist;
                $scope.ReceiptLR.noofbundels = 0;
                $scope.ReceiptLR.frieghtcharges = 0;
                $scope.ReceiptLR.noofpieces = 0;
                $scope.ReceiptLR.noofboxes = 0;
                $scope.ReceiptLR.actualweight = 0;
                $scope.ReceiptLR.chargedweight = 0;
                $scope.ReceiptLR.noofbundelsreceived = 0;
                $scope.ReceiptLR.noofboxesreceived = 0;
                $.each(result[0].lrissuelist, function (k, v) {

                    v.chargedweight = v.weight;
                    v.actualweight = v.weight;

                    if (v.computedfrightcharge > 0) {
                        v.frightcharge = v.computedfrightcharge;
                        v.perkg = v.computedfrightcharge / v.weight;
                        if (v.computedfrightcharge != v.totalamount)
                            v.style = { "background-color": "lightgrey" };
                    }
                    else {
                        v.frightcharge = v.totalamount;
                        v.perkg = 0;
                    }

                    v.loadingcharge = v.computedloadingcharge;

                    $scope.ReceiptLR.loadingcharges += v.loadingcharge;
                    $scope.ReceiptLR.noofbundels += v.noofbundels;
                    $scope.ReceiptLR.frieghtcharges += v.frightcharge;
                    $scope.ReceiptLR.noofpieces += v.noofpieces;
                    $scope.ReceiptLR.noofboxes += v.noofboxes;
                    $scope.ReceiptLR.noofbundelsreceived += v.noofbundels;
                    $scope.ReceiptLR.noofboxesreceived += v.noofboxes;

                    $scope.ReceiptLR.actualweight += v.weight;
                    $scope.ReceiptLR.chargedweight += v.weight;
                    v.noofbundelsreceived = v.noofbundels;
                    v.noofboxesreceived = v.noofboxes;
                });

                $scope.ConfigureGrid();
                $scope.igrid.UpdateData($scope.ReceiptLR.receiptitem, '_id');

            }).error(function (result) {
            });
        }
        else if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": { 'id': $location.search().id },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrreceivedService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ReceiptLR = result[0];

                    $scope.ConfigureGrid();
                    $scope.igrid.UpdateData($scope.ReceiptLR.receiptitem, '_id');
                }
            }).error(function (result) {
            });
        } else {
            $timeout(function () {
                $scope.ConfigureGrid();
            }, 10);
        }

        $scope.ConfigureGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 80, $scope.FromLocations);

            igrid.AddSimpleColumn('perkg', 'Per KG', 60);
            igrid.AddSimpleColumn('lrno', 'LR No', 80);
            igrid.AddFormatedColumn('totalamount', 'Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('actualweight', 'Weight', 80, 'N2', 2).total = true;
            igrid.AddCustomColumn('edit', 'Reverse', 60, function (item, col) {
                if (item.isreversed)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });

            igrid.AddColumn(
                {
                    id: "noofbundelsreceived", name: "Bundle", field: "noofbundelsreceived",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "noofboxesreceived", name: "Boxes", field: "noofboxesreceived",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "chargedweight", name: "Charged Weight", field: "chargedweight",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "receivedweight", name: "Received Weight", field: "receivedweight",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "frightcharge", name: "Fright Charge", field: "frightcharge",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "insurance", name: "Insurance", field: "insurance",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            igrid.AddColumn(
                {
                    id: "packing", name: "Packing", field: "packing",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "loadingcharge", name: "Loading", field: "loadingcharge",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "forward", name: "Forward", field: "forward",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddColumn(
                {
                    id: "others", name: "Other", field: "others",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );



            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'isreversed') {
                    item = args.grid.getDataItem(args.row);
                    item.isreversed = !item.isreversed && $(e.target).val() == 'on';
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                    $scope.computecharge(item, args.grid.getColumns()[args.cell].id);
                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var column = args.grid.getColumns()[args.cell].id;
                if (column == 'chargedweight') {
                    $scope.computefright(item, column);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else {
                    $scope.computecharge(item, column);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.igrid = igrid;
        }

        $scope.CreateNew = function () {

            $scope.ReceiptLR.receivedon = new Date();
        }();

        
    }]);