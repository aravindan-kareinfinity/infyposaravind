var app = angular.module('InfinityLab');

app.controller('BinStorageController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;


        $scope.Entity = {
            name: "",
            printingname: "",
            categoryid: "",
            uomid: "",
            colorid: "",
            width: "",
            taxid: ""
        };


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
        $scope.Clipboard = {};
        $scope['bintree'] = $scope['bintree'] || {};
        $scope['bintree'].onCurrentNodeChange = function (node) {
            $scope.CurrentNode = node;
            if (node.supportallocation) {
                $scope.ShowList();
            }
            else {
                if ($scope.sgrid != null)
                    $scope.sgrid.destroy();
                $scope.sgrid = null;
            }
        }

        $scope.OnContextMenuSelected = function (item, menu, style) {
            $timeout(function () {

                if (typeof ($scope.bintree.currentNode) == 'undefined')
                    return;
                var currentnode = $scope.bintree.currentNode;

                if (menu == 'Edit' && currentnode.level == 0)
                    return;


                if (menu == 'Delete') {
                    if (typeof (currentnode.parentnode) != 'undefined') {
                        currentnode.parentnode.children.splice(currentnode.parentnode.children.indexOf(currentnode), 1);
                        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
                    }
                    return;
                }

                if (menu == 'Copy') {
                    if (typeof (currentnode.parentnode) != 'undefined') {
                        $scope.Clipboard = { 'Copy': currentnode };
                    }
                    return;
                }

                if (menu == 'Cut') {
                    if (typeof (currentnode.parentnode) != 'undefined') {
                        currentnode.parentnode.children.splice(currentnode.parentnode.children.indexOf(currentnode), 1);
                        $scope.Clipboard = { 'Cut': currentnode };
                        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
                    }
                    return;
                }

                if (menu == 'Paste') {
                    if (typeof (currentnode.parentnode) != 'undefined') {

                        if (typeof ($scope.Clipboard.Copy) != 'undefined') {
                            currentnode.parentnode.children.push($scope.Clipboard.Copy);
                            $scope.Clipboard = {};
                        }
                        if (typeof ($scope.Clipboard.Cut) != 'undefined') {
                            currentnode.parentnode.children.push($scope.Clipboard.Cut);
                            $scope.Clipboard = {};
                        }
                        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
                    }
                    return;
                }


                var node = currentnode;
                if (menu != 'Edit')
                    node = {
                        'nodetype': menu, name: menu, code: menu,
                        'level': node.level + 1, parentnode: node,
                        "stylecollapsed": style + '_collapsed',
                        "styleexpanded": style + '_expanded',
                        "stylenormal": style + '_normal',
                        "label": menu,
                        "children": []
                    };

                var dialog = ngDialog.open({
                    template: 'BinConfig',
                    data: [$scope, item, menu, node],
                    width: '400px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.parentScope = $scope.ngDialogData[0];
                        $scope.item = $scope.ngDialogData[1];
                        $scope.menu = $scope.ngDialogData[2];
                        $scope.node = $scope.ngDialogData[3];
                        $scope.Selected = function (item) {
                            $scope.closeThisDialog({ 'node': $scope.node, 'menu': $scope.menu });
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                            && data.value != '$document') {
                            if (data.value.menu == 'Edit') return;
                            data.value.node.parentnode.children.push(data.value.node);
                        }
                    }
                });
            }, 10);
        }

        $scope.DeleteList = function () {
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;
            var currentnode = $scope.bintree.currentNode;
            var list = this.sgrid.GetSelectedRows();
            if (list.length > 0) {
                $scope.bintreedata[0].DeletedBinList = $scope.bintreedata[0].DeletedBinList || [];
                $.each(list, function (k, v) {
                    if (typeof (v.id) != 'undefined' && v.id > 0)
                        $scope.bintreedata[0].DeletedBinList.push(v.id);
                    currentnode.binlist.splice(currentnode.binlist.indexOf(v), 1);
                });
                $scope.sgrid.UpdateData(currentnode.binlist);
            }
        }

        $scope.BinGenerator = function () {
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;
            var currentnode = $scope.bintree.currentNode;

            var dialog = ngDialog.open({
                template: 'BinGenerator',
                data: [$scope, currentnode],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parentScope = $scope.ngDialogData[0];
                    $scope.node = $scope.ngDialogData[1];
                    $scope.CodeSource = [
                        { 'id': 1, 'name': 'Row' },
                        { 'id': 2, 'name': 'Column' }
                    ];
                    $scope.Selected = function (item) {

                        var row = parseInt($scope.node.rows);
                        var column = parseInt($scope.node.columns);
                        var columnlength = parseInt($scope.node.columnlength);
                        var rowlength = parseInt($scope.node.rowlength);
                        if (isNaN(rowlength)) rowlength = 0;
                        if (isNaN(columnlength)) columnlength = 0;
                        var rowprefix = typeof ($scope.node.rowprefix) == 'undefined' ? '' : $scope.node.rowprefix;
                        var columnprefix = typeof ($scope.node.columnprefix) == 'undefined' ? '' : $scope.node.columnprefix;
                        var prefix = typeof ($scope.node.codeprefix) == 'undefined' ? '' : $scope.node.codeprefix;
                        var suffix = typeof ($scope.node.codesuffix) == 'undefined' ? '' : $scope.node.codesuffix;
                        if (isNaN(row) || isNaN(column) || column <= 0 || row <= 0) {
                            alert('Give the correct configuration');
                            return;
                        }
                        var node = $scope.node;
                        node.binlist = node.binlist || [];
                        for (var r = 1; r <= row; r++) {
                            for (var c = 1; c <= column; c++) {
                                var code = prefix;
                                if (node.codefirstpart == 1)
                                    code += rowprefix + r.toString().padEnd('0', rowlength);
                                if (node.codefirstpart == 2)
                                    code += columnprefix + c.toString().padEnd('0', columnlength);

                                if (node.codesecondpart == 1)
                                    code += rowprefix + r.toString().padEnd('0', rowlength);
                                if (node.codesecondpart == 2)
                                    code += columnprefix + c.toString().padEnd('0', columnlength);
                                code += suffix;
                                var exist = node.binlist.find(function (item) {
                                    return item.row == r && item.column == c;
                                });
                                if (typeof (exist) == 'undefined' || exist == null) {
                                    node.binlist.push({
                                        row: r, column: c, code: code,
                                        maxqty: $scope.maxqty, height: $scope.node.binheight,
                                        width: $scope.node.binwidth, 'altered': false
                                    });
                                }
                                else {
                                    exist.maxqty = $scope.maxqty;
                                    exist.code = $scope.code;
                                    exist.height = $scope.binheight;
                                    exist.width = $scope.binwidth;
                                    exist.altered = true;
                                }
                            }
                        }
                        $scope.closeThisDialog({ 'node': node, 'menu': $scope.menu });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $scope.ShowList();
                        return;
                    }
                }
            });
        }

        $scope.ShowStockList = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('uid', 'ID', 50);
            igrid.AddSimpleColumn('designname', 'Design', 50);
            igrid.AddSimpleColumn('product', 'Product', 80);
            igrid.AddSimpleColumn('iqty', 'Qty', 80);
            igrid.AddSimpleColumn('code', 'Code', 80);
            
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Outward'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Outward') {
                        
                    }
                    else if ($(e.target).val() == 'Delete') {

                    }
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;

            var currentnode = $scope.bintree.currentNode;
            $scope.sgrid.UpdateData(currentnode.binlist);
        }

        $scope.ShowList = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('id', 'ID', 50);
            igrid.AddSimpleColumn('row', 'Row', 80);
            igrid.AddSimpleColumn('column', 'Column', 80);
            igrid.AddSimpleColumn('code', 'Code', 80);
            igrid.AddSimpleColumn('maxqty', 'Qty', 80);
            igrid.AddSimpleColumn('width', 'Width', 80);
            igrid.AddSimpleColumn('height', 'Height', 80);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Change'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Change') {
                        $scope.EditBinItem(args.item);
                    }
                    else if ($(e.target).val() == 'Delete') {
                        if (typeof ($scope.bintree.currentNode) == 'undefined')
                            return;
                        var currentnode = $scope.bintree.currentNode;

                        $scope.bintreedata[0].DeletedBinList = $scope.bintreedata[0].DeletedBinList || [];
                        if (typeof (args.item.id) != 'undefined' && args.item.id > 0) {
                            $scope.bintreedata[0].DeletedBinList.push(args.item.id);
                            currentnode.binlist.splice(currentnode.binlist.indexOf(args.item), 1);
                            $scope.sgrid.UpdateData(currentnode.binlist);
                        }
                    }
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;

            var currentnode = $scope.bintree.currentNode;
            $scope.sgrid.UpdateData(currentnode.binlist);
        }

        $scope.EditBinItem = function (item) {

            var dialog = ngDialog.open({
                template: 'BinEdit',
                data: [$scope, item],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parentScope = $scope.ngDialogData[0];
                    $scope.node = $scope.ngDialogData[1];
                    $scope.Selected = function (item) {
                        $scope.closeThisDialog({ 'node': node, 'menu': $scope.menu });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $scope.ShowList();
                        return;
                    }
                }
            });

        }

        $scope.LoadConfiguration = function () {
            restService.Get('ReferencelistService/BINHIERARCHY/' + $scope.Binstorage.locationid, true).then(function (results) {
                if (results.length == 0) {

                    var name = $scope.SearchRef($scope.Binstorage.locationid, $scope.Location);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": 0,
                                "referencelocationid": $scope.Binstorage.locationid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "supportchild": true,
                                "supportallocation": false,
                                "level": 0,
                                "nodetype": "location",
                                "children": [],
                                "isactive": true,
                                "version":1
                            }
                        ];


                }
                else {
                    var name = $scope.SearchRef($scope.Binstorage.locationid, $scope.Location);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": results[0].id,
                                "referencelocationid": $scope.Binstorage.locationid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "supportchild": true,
                                "supportallocation": false,
                                "level": 0,
                                "isactive": true,
                                "version": results[0].version,
                                "nodetype": "location",
                                "children": results[0].children
                            }
                        ];
                }
                $scope.AssignParent($scope.bintreedata[0], null);
            });
        }

        $scope.allmenuOptions = [
            ['Create Building', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Building', 'building');
            }],
            ['Create Floor', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Floor', 'floor');
            }],
            ['Create Room', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Room', 'room');
            }],
            ['Create Partition', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Partion', 'bin');
            }],
            ['Edit', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Edit', 'edit');
            }],
            ['Delete', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Delete', 'delete');
            }],
            ['Cut', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Cut', 'cut');
            }],
            ['Paste', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Paste', 'paste');
            }]
        ];

        $scope.ErrorMessage = "";

        $scope.AssignParent = function (node, parentnode) {
            node.parentnode = parentnode;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.AssignParent(v, node);
                })
            }
        }

        $scope.RemoveParent = function (node) {
            node.parentnode = null;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.RemoveParent(v)
                })
            }
        }

        $scope.Save = function () {
            $scope.RemoveParent($scope.bintreedata[0]);
            var item = (JSON.parse(JSON.stringify($scope.bintreedata[0])));
            request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/SaveBinStorage",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $scope.bintreedata[0].id = result.id;
                    $scope.bintreedata[0].DeletedBinList = [];
                    $scope.bintreedata[0].version = result.version;
                    $scope.bintreedata[0].referencelocationid = result.referencelocationid;
                    $scope.bintreedata[0].children = result.children;
                    $scope.AssignParent($scope.bintreedata[0], null);
                    $rootScope.Toaster.Info("Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    url: "BOMMasterService/Delete",
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.BOMMaster = item;
                $scope.Navigation.SetMode('EDIT');
            }
        }

        $scope.bintreedata = [];
        //[
        //    {
        //        "label": "User", "id": "role1", "stylecollapsed": 'collapsed',
        //        "styleexpanded": 'expanded', "stylenormal": 'normal',
        //        "children": [
        //            {
        //                "label": "subUser1", "id": "role11", "stylecollapsed": 'collapsed',
        //                "styleexpanded": 'expanded', "stylenormal": 'normal', "children": [] },
        //            {
        //                "label": "subUser2", "id": "role12", "stylecollapsed": 'collapsed',
        //                "styleexpanded": 'expanded', "stylenormal": 'normal', "children": [
        //                    {
        //                        "label": "subUser2-1", "id": "role121", "children": [
        //                            { "label": "subUser2-1-1", "id": "role1211", "children": [] },
        //                            { "label": "subUser2-1-2", "id": "role1212", "children": [] }
        //                        ]
        //                    }
        //                ]
        //            }
        //        ]
        //    },
        //    { "label": "Admin", "id": "role2", "children": [] },
        //    { "label": "Guest", "id": "role3", "children": [] }
        //];
        $scope.panelheight = { 'height': '400px' };
        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 100) + 'px';
            }
        }, 100);


    }]);