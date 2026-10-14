function InfyGrid(gridcontainer, height, width, enablePagination, enableSelector, Ex, additonalheight) {
    this.data = [];
    this.columns = [];
    this.Ex = typeof (Ex) != 'undefined' && Ex;
    this.columnFilters = [];
    this.columnTypes = [];
    this.enableTotal = false;
    this.enableSerial = false;

    this.parseInt = function (value) {
        if (typeof (value) == 'undefined' || value == null || value == '')
            return 0;

        value = value.toString().replace('px', '').replace('PX', '');
        var pi = parseInt(value);
        if (isNaN(pi))
            return 0;
        return pi;
    }


    this.options = {
        editable: true,
        enableAddRow: false,
        enableCellNavigation: true,
        explicitInitialization: true,
        enableColumnReorder: false,
        autoEdit: false,
        forceFitColumns: false,
        headerRowHeight: 30,
        multiColumnSort: true,
        showHeaderRow: false,
        rowHeight: 30,
        enableAsyncPostRender: false,
        asyncEditorLoading: false,
        additonalheight: this.parseInt(additonalheight),
        baseheight: this.parseInt(height)
    };


    this.onScroll = function (e, args) {

    }
    this.onRowValueChanged = function (e, args) {

    }

    this.onPageInfoChanged = function (e, PageInfo) {

    }

    this.UpdateAllTotals = function (grid) {
        var columnIdx = grid.getColumns().length;
        while (columnIdx--) {
            if (typeof (grid.getColumns()[columnIdx].total) != 'undefined' && grid.getColumns()[columnIdx].total)
                this.UpdateTotal(columnIdx, grid);
            else if (typeof (grid.getColumns()[columnIdx].average) != 'undefined' && grid.getColumns()[columnIdx].average)
                this.UpdateAverage(columnIdx, grid);
        }
    }

    this.GetNumber = function (value) {
        if (typeof (value) == 'undefined' || value == null) return 0;
        if (value.toString().indexOf(',') > 0) {
            value = value.toString().replace(',', '');
        }
        return (parseFloat(value, 10) || 0);
    }

    this.UpdateAverage = function (cell, grid, onlyvalue) {
        var columnId = grid.getColumns()[cell].id;
        var column = grid.getColumns()[cell];
        var data = grid.getData();
        var total = 0;
        var i = data.getLength();
        while (i--) {
            var item = grid.getDataItem(i);
            var value = typeof (column.getValue) != 'undefined' ? column.getValue(item, column) : this.GetNumber(item[column.field]);
            value = parseFloat(value);
            if (!isNaN(value) && isFinite(value))
                total += value;
        }
        var columnElement = grid.getFooterRowColumn(columnId);
        total = (total / data.getLength()).round(2);
        if (isNaN(total) || !isFinite(total)) total = 0;
        $(columnElement).html("<div class='rightalign' style='float: right;'>" + this.FormatNumber(total, column.decimal || 0) + "</div>");
        if (onlyvalue) return total;
        return this.FormatNumber(total, column.decimal || 2);
    }

    this.UpdateTotal = function (cell, grid, onlyvalue) {
        var columnId = grid.getColumns()[cell].id;
        var column = grid.getColumns()[cell];
        var data = grid.getData();
        var total = 0;
        var i = data.getLength();
        while (i--) {
            var item = grid.getDataItem(i);
            var value = typeof (column.getValue) != 'undefined' ? column.getValue(item, column) : this.GetNumber(item[column.field]);
            value = parseFloat(value);
            if (!isNaN(value) && isFinite(value))
                total += value;
        }
        var columnElement = grid.getFooterRowColumn(columnId);
        $(columnElement).html("<div class='rightalign' style='float: right;'>" + this.FormatNumber(total, column.decimal || 0) + "</div>");
        if (onlyvalue) return total;
        return this.FormatNumber(total, column.decimal || 0);
    }

    this.FormatNumber = function (value, decimal) {
        value = parseFloat(value);
        var svalue = value.toFixed(decimal);

        var values = svalue.toString().split('.');

        var leftvalue = values[0].toString().replace(/(\d)(?=(\d\d)+\d$)/g, "$1,");
        if (values.length == 2)
            return leftvalue + "." + values[1];
        return leftvalue;
    }

    this.EnableTotal = function () {
        this.options.createFooterRow = true;
        this.options.showFooterRow = true;
        this.options.footerRowHeight = 21;
        this.enableTotal = true;
    }

    this.EnableSerial = function (data, addcolumn) {
        $.each(data, function (k, v) {
            v.serial = k + 1;
        });
        if (typeof (addcolumn) == 'undefined' || addcolumn)
            this.AddFormatedColumn('serial', 'S.No', 40, 'N0', 0);
    }

    this.getInnerText = function (htmlString) {
        // Create a temporary DOM element
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = htmlString;

        // Return the inner text of the first child
        return tempDiv.textContent || tempDiv.innerText || '';
    }

    this.ExportXLSX = function (grid, title, subtitle, allrows, additionalrows) {

        var worksheetName = 'Quanto Report';

        var processRow = function (row) {
            var finalVal = '';
            for (var j = 0; j < row.length; j++) {
                var innerValue = row[j] == undefined || row[j] === null ? '' : row[j].toString();
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
        var colname = [];
        var skipIndex = [];
        var formator = [];
        var groups = typeof (grid.getGroupInfo) != 'undefined' ? grid.getGroupInfo() : null;

        var colcount = 0;
        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (!(cname.startsWith('<') ||
                cname == '' ||
                (cname.buttontext && cname.buttontext.startsWith('<')))) {
                colcount++
            }
        }

        /*var $rootsScope = $('body').scope('$rootsScope');
        if (typeof ($rootsScope) != 'undefined' && $rootsScope.UserInfo.CompanyName != '') {
            rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + $rootsScope.UserInfo.CompanyName + "</th></tr>'");
        }*/


        var colwidths = [];
        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (cname == '' || cname.startsWith('<')) {
                if (grid.getColumns()[j].field == 'sel' || grid.getColumns()[j].field == undefined || grid.getColumns()[j].field == null || grid.getColumns()[j].field == '') {
                    skipIndex.push(j);
                }
                else {
                    colname.push(grid.getColumns()[j].name);
                    colwidths.push({ 'width': (grid.getColumns()[j].width / 5) });
                }
            }
            else if (grid.getColumns()[j].buttontext && grid.getColumns()[j].buttontext.startsWith('<')) {
                if (grid.getColumns()[j].field == undefined || grid.getColumns()[j].field == null || grid.getColumns()[j].field == '') {
                    skipIndex.push(j);
                }
                else {
                    colname.push(grid.getColumns()[j].name);
                    colwidths.push({ 'width': (grid.getColumns()[j].width / 5) });
                }
            }
            else {
                colname.push(grid.getColumns()[j].name);
                colwidths.push({ 'width': (grid.getColumns()[j].width / 5) });
            }
        }

        groupcolumn = [];
        if (groups != null) {
            for (var j = 0, jlen = groups.length; j < jlen; j++) {
                for (var k = 0, klen = groups[j].length; k < klen; k++) {
                    var columns = groups[j][k].extractColumns();;
                    var alength = columns.length;
                    for (var cj = 0; cj < columns.length; cj++) {
                        if (skipIndex.indexOf(cj) >= 0)
                            alength--;
                    }
                    groupcolumn.push({ 'condent': groups[j][k].name, 'colspan': alength });
                }
            }
        }

        if (typeof (title) != 'undefined' && title != null && title.length > 0) {
            rows.push([{ 'condent': title, 'colspan': colname.length - 1 }]);
        }

        var headerlength = 0;
        if (typeof (subtitle) != 'undefined' && subtitle != null && subtitle.length > 0) {
            if ($.isArray(subtitle)) {
                var subgroupcolumn = [];
                $.each(subtitle, function (k, v) {
                    if ($.isArray(v)) {
                        $.each(v, function (k, vv) {
                            subgroupcolumn.push({ 'condent': vv, 'colspan': 1 });
                        })
                    }
                    else {
                        subgroupcolumn.push({ 'condent': v, 'colspan': 1 });
                    }
                })
                rows.push(subgroupcolumn);
            }
            else {
                rows.push([{ 'condent': subtitle, 'colspan': colname.length - 1 }]);
            }
        }

        if (groupcolumn.length > 0)
            rows.push(groupcolumn);
        rows.push(colname);
        headerlength = rows.length;

        var selectedIndexes = grid.getSelectedRows();
        if (allrows)
            selectedIndexes = false;
        var singlerow = [];
        for (var i = 0, l = grid.getData().getLength(); i < l; i++) {
            if (selectedIndexes && selectedIndexes.length > 0 &&
                selectedIndexes.indexOf(i) < 0)
                continue;


            for (var j = 0, len = grid.getColumns().length; j < len; j++) {

                if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;
                //singlerow.push(grid.getDataItem(i)[grid.getColumns()[j].field]);
                var item = grid.getDataItem(i);
                var column = grid.getColumns()[j];
                var value = item[column.field];
                // Added 
                if (typeof (column.getValue) != 'undefined')
                    value = column.getValue(item, column);
                //
                if (typeof (value) == 'undefined') {
                    if (typeof (column.getValue) != 'undefined')
                        value = column.getValue(item, column);
                }
                if (typeof (value) == 'undefined') {
                    if (typeof (column.formatevent) != 'undefined')
                        value = column.formatevent(item, column);
                }
                if (column.filtertype == 'number' || column.total || column.average) {
                    if (column.decimal && column.decimal > 0) {
                        singlerow.push(this.GetNumber(value).round(column.decimal));
                    }
                    else {
                        if (value && value != null && value.indexOf && value.indexOf('&nbsp;') >= 0) {
                            value = value.replaceAll('&nbsp;', ' ');
                        }
                        singlerow.push(value);
                    }
                }

                else if (column.formatter && column.formatter != null) {
                    var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
                    if (fvalue && fvalue != null && fvalue.indexOf && fvalue.indexOf('&nbsp;') >= 0) {
                        fvalue = fvalue.replaceAll('&nbsp;', ' ');
                    }
                    if (fvalue && fvalue != null && fvalue.indexOf('<') >= 0 && fvalue.indexOf('>') > 0) {
                        fvalue = this.getInnerText(fvalue);
                    }
                    singlerow.push(fvalue);
                }
                else if (value && value != null) {
                    fvalue = value.replaceAll('&nbsp;', ' ');
                    if (fvalue && fvalue != null && fvalue.indexOf('<') >= 0 && fvalue.indexOf('>') > 0) {
                        fvalue = this.getInnerText(fvalue);
                    }
                    singlerow.push(fvalue);
                }
                else {
                    singlerow.push('');
                }
            }
            rows.push(singlerow);
            singlerow = [];
        }

        var totalrowidx = -1;
        if (this.enableTotal) {
            var singlerow = [];
            for (var columnIdx = 0; columnIdx < grid.getColumns().length; columnIdx++) {
                if (skipIndex.length > 0 && skipIndex.indexOf(columnIdx) != -1) continue;

                if (typeof (grid.getColumns()[columnIdx].total) != 'undefined'
                    && grid.getColumns()[columnIdx].total)
                    singlerow.push(this.UpdateTotal(columnIdx, grid, true));
                else if (typeof (grid.getColumns()[columnIdx].average) != 'undefined'
                    && grid.getColumns()[columnIdx].average)
                    singlerow.push(this.UpdateAverage(columnIdx, grid, true));
                else
                    singlerow.push('');
            }
            rows.push(singlerow);
            singlerow = [];
            totalrowidx = rows.length;
        }

        if (additionalrows &&
            additionalrows != null &&
            additionalrows.length > 0) {
            for (var i = 0; i < additionalrows.length; i++) {

                for (var j = 0, len = grid.getColumns().length; j < len; j++) {

                    if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;
                    if (additionalrows[i][j] == undefined) continue;
                    var item = grid.getDataItem(i);
                    var column = grid.getColumns()[j];
                    var value = additionalrows[i][j];

                    if (column.filtertype == 'number' || column.total || column.average) {
                        if (column.decimal && column.decimal > 0) {
                            singlerow.push(this.GetNumber(value).round(column.decimal));
                        }
                        else {
                            singlerow.push(value);
                        }
                    }
                    else {
                        var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
                        singlerow.push(fvalue);
                    }
                }
                rows.push(singlerow);
                singlerow = [];
            }
        }

        var data = [];
        var meregelist = [];
        $.each(rows, function (k, v) {
            var cols = [];
            $.each(v, function (kk, vv) {
                if (vv && vv.colspan) {
                    cols.push(vv.condent);
                    var merge = { s: { r: k, c: kk }, e: { r: k, c: kk + vv.colspan } };
                    meregelist.push(merge);
                }
                else if (vv) {
                    cols.push(vv);
                } else {
                    cols.push('');
                }
            })
            data.push(cols);
        });

        /* merge cells A1:B1 */
        //var merge = { s: { r: 0, c: 0 }, e: { r: 0, c: 1 } };
        //var merge = XLSX.utils.decode_range("A1:B1"); // this is equivalent

        /* generate worksheet */
        var ws = XLSX.utils.aoa_to_sheet(data);
        ws['!merges'] = meregelist;
        var titlestyle = {									// set the style for target cell
            font: {
                name: 'verdana',
                sz: 12,
                bold: true,
                color: { rgb: "33220000" }
            },
            alignment: {
                vertical: "center",
                horizontal: "center"
            }
        };
        var totalstyle = {									// set the style for target cell
            font: {
                name: 'verdana',
                sz: 12,
                bold: true,
                color: { rgb: "33220000" }
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
            },
            alignment: {
                vertical: "center",
                horizontal: "right"
            }
        };
        var headerstyle = {									// set the style for target cell
            font: {
                name: 'verdana',
                sz: 11,
                bold: true,
                color: { rgb: "33220000" }
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
            },
            alignment: {
                vertical: "center",
                horizontal: "center"
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

        ws['!cols'] = colwidths;

        for (var ridx = 1; ridx < headerlength; ridx++)
            ws["A" + ridx].s = titlestyle;

        var prefix = '';
        var startfrom = 64;
        for (var ridx = 1; ridx <= colname.length; ridx++) {
            startfrom++;
            ws[prefix + String.fromCharCode(startfrom) + headerlength].s = headerstyle;
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


        if (totalrowidx > 0) {
            prefix = '';
            startfrom = 64;
            for (var ridx = 1; ridx <= colname.length; ridx++) {
                startfrom++;
                ws[prefix + String.fromCharCode(startfrom) + totalrowidx].s = totalstyle;
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
        }

        prefix = '';
        startfrom = 64;
        for (var ridx = 1; ridx <= colname.length; ridx++) {
            startfrom++;
            for (var rnidx = headerlength + 1; rnidx < totalrowidx; rnidx++) {
                if (ws[prefix + String.fromCharCode(startfrom) + rnidx])
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

        /* add merges */
        //if (!ws['!merges']) ws['!merges'] = [];
        //ws['!merges'].push(merge);

        //ws["A1"].s = {									// set the style for target cell
        //    font: {
        //        name: 'Calibri',
        //        sz: 24,
        //        bold: true,
        //        color: { rgb: "FFFFAA00" }
        //    },
        //};

        /* generate workbook */
        var wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "sheet1");


        /* generate file and download */
        const wbout = XLSX.write(wb, { type: "array", bookType: "xlsx" });
        saveAs(new Blob([wbout], { type: "application/octet-stream" }), "download.xlsx");
    }

    this.ExportExcel = function (grid, title, subtitle, allrows, additionalrows, footerfunc) {

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
        var groups = typeof (grid.getGroupInfo) != 'undefined' ? grid.getGroupInfo() : null;

        var colcount = 0;
        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (!(cname.startsWith('<') ||
                //cname == '' ||
                (cname.buttontext && cname.buttontext.startsWith('<')))) {
                colcount++
            }
        }

        /*var $rootsScope = $('body').scope('$rootsScope');
        if (typeof ($rootsScope) != 'undefined' && $rootsScope.UserInfo.CompanyName != '') {
            rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + $rootsScope.UserInfo.CompanyName + "</th></tr>'");
        }*/

        if (typeof (title) != 'undefined' && title != null && title.length > 0) {
            rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + title + "</th></tr>");
        }

        if (typeof (subtitle) != 'undefined' && subtitle != null && subtitle.length > 0) {

            if ($.isArray(subtitle)) {
                rows.push('<Tr>')
                groupcolumn = [];
                $.each(subtitle, function (k, v) {
                    if ($.isArray(v)) {
                        $.each(v, function (k, vv) {
                            groupcolumn.push("<th style=\"mso-number-format:'\@'\">");
                            groupcolumn.push(vv);
                            groupcolumn.push('</th>');
                        })
                    }
                    else {
                        groupcolumn.push("<th style=\"mso-number-format:'\@'\">");
                        groupcolumn.push(v);
                        groupcolumn.push('</th>');
                    }
                })
                rows.push(groupcolumn.join(''));
                rows.push('</Tr>')
            }
            else {
                rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + subtitle + "</th></tr>");
            }
        }

        if (groups != null) {
            groupcolumn = [];
            for (var j = 0, jlen = groups.length; j < jlen; j++) {
                rows.push('<Tr>')
                for (var k = 0, klen = groups[j].length; k < klen; k++) {

                    var columns = groups[j][k].extractColumns();;
                    var alength = columns.length;
                    for (var cj = 0; cj < columns.length; cj++) {
                        var cname = columns[cj].name;
                        if (cname.startsWith('<')) {
                            alength--;
                        }
                    }
                    groupcolumn.push("<th style=\"mso-number-format:'\@'\" colspan='" + alength + "'>");
                    groupcolumn.push(groups[j][k].name);
                    groupcolumn.push('</th>');
                }
                rows.push(groupcolumn.join(''));
                rows.push('</Tr>')
            }
        }

        rows.push('<Tr>')
        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (cname.startsWith('<') || (cname.buttontext && cname.buttontext.startsWith('<'))) {
                skipIndex.push(j);
            }
            else {
                colname.push("<th style=\"mso-number-format:'\@'\">");
                colname.push(grid.getColumns()[j].name);
                colname.push('</th>');
            }
        }
        rows.push(colname.join(''));
        rows.push('</Tr></Thead><Tbody>')

        var selectedIndexes = grid.getSelectedRows();
        if (allrows)
            selectedIndexes = false;
        var singlerow = [];
        for (var i = 0, l = grid.getData().getLength(); i < l; i++) {
            if (selectedIndexes && selectedIndexes.length > 0 &&
                selectedIndexes.indexOf(i) < 0)
                continue;


            for (var j = 0, len = grid.getColumns().length; j < len; j++) {

                if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;


                //singlerow.push(grid.getDataItem(i)[grid.getColumns()[j].field]);
                var item = grid.getDataItem(i);
                var column = grid.getColumns()[j];
                var value = item[column.field];
                // Added 
                if (typeof (column.getValue) != 'undefined')
                    value = column.getValue(item, column);
                //
                if (typeof (value) == 'undefined') {
                    if (typeof (column.getValue) != 'undefined')
                        value = column.getValue(item, column);
                }
                if (typeof (value) == 'undefined') {
                    if (typeof (column.formatevent) != 'undefined')
                        value = column.formatevent(item, column);
                }
                if (column.filtertype == 'number' || column.total || column.average) {
                    if (column.decimal && column.decimal > 0) {
                        singlerow.push('<td mso-number-format:"0.' + '0'.repeat(column.decimal) + '">');
                        singlerow.push(this.GetNumber(value).round(column.decimal));
                    }
                    else {
                        singlerow.push("<td style=\"mso-number-format:'0'\">");
                        singlerow.push(value);
                    }
                }
                else {
                    singlerow.push("<td style=\"mso-number-format:'\@'\">");
                    var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
                    singlerow.push(fvalue);
                }
                singlerow.push('</td>');
            }
            rows.push('<tr>');
            rows.push(singlerow.join(''));
            rows.push('</tr>');
            singlerow = [];
        }


        rows.push('</Tbody>')

        rows.push('<thead>')
        if (this.enableTotal) {
            var singlerow = [];

            for (var columnIdx = 0; columnIdx < grid.getColumns().length; columnIdx++) {
                if (skipIndex.length > 0 && skipIndex.indexOf(columnIdx) != -1) continue;
                singlerow.push("<th style = 'text-align:right'>");
                if (typeof (grid.getColumns()[columnIdx].total) != 'undefined'
                    && grid.getColumns()[columnIdx].total)
                    singlerow.push(this.UpdateTotal(columnIdx, grid, true));
                if (typeof (grid.getColumns()[columnIdx].average) != 'undefined'
                    && grid.getColumns()[columnIdx].average)
                    singlerow.push(this.UpdateAverage(columnIdx, grid, true));
                singlerow.push('</th>');
            }

            rows.push('<tr>');
            rows.push(singlerow.join(''));
            rows.push('</tr>');
            singlerow = [];
        }

        if (additionalrows &&
            additionalrows != null &&
            additionalrows.length > 0) {
            for (var i = 0; i < additionalrows.length; i++) {

                for (var j = 0, len = grid.getColumns().length; j < len; j++) {

                    if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;
                    if (additionalrows[i][j] == undefined) continue;
                    var item = grid.getDataItem(i);
                    var column = grid.getColumns()[j];
                    var value = additionalrows[i][j];

                    if (column.filtertype == 'number' || column.total || column.average) {
                        if (column.decimal && column.decimal > 0) {
                            singlerow.push('<td mso-number-format:"0.' + '0'.repeat(column.decimal) + '">');
                            singlerow.push(this.GetNumber(value).round(column.decimal));
                        }
                        else {
                            singlerow.push("<td style=\"mso-number-format:'0'\">");
                            singlerow.push(value);
                        }
                    }
                    else {
                        singlerow.push("<td style=\"mso-number-format:'\@'\">");
                        var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
                        singlerow.push(fvalue);
                    }
                    singlerow.push('</td>');
                }
                rows.push('<tr>');
                rows.push(singlerow.join(''));
                rows.push('</tr>');
                singlerow = [];
            }
        }

        if (footerfunc && footerfunc != null) {
            footerfunc(rows);
        }

        rows.push('</thead>')

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
        return href;

        //var link = document.createElement("a");
        //link.setAttribute("href", href);
        //link.setAttribute("download", "filename.csv");
        //link.style.visibility = 'hidden';
        //document.body.appendChild(link);
        //link.click();
        //document.body.removeChild(link);


    }

    this.ExportCSV = function (grid, title, subtitle, allrows, additionalrows) {


        var processRow = function (row) {
            var finalVal = '';
            for (var j = 0; j < row.length; j++) {
                var innerValue = row[j] == undefined || row[j] === null ? '' : row[j].toString();
                if (row[j] instanceof Date) {
                    innerValue = row[j].toLocaleString();
                };
                var result = innerValue.replace(/"/g, '').replace(/,/g, '').replace(/\r/g, '').replace(/\n/g, '');

                if (j > 0)
                    finalVal += ',';
                finalVal += result;
            }
            return finalVal;
        };

        var csvFile = '';
        var rows = [];
        var colname = [];
        var skipIndex = [];
        var formator = [];

        if (title && title != null)
            rows.push(title);
        if (subtitle && subtitle != null)
            rows.push(title);

        var groups = typeof (grid.getGroupInfo) != 'undefined' ? grid.getGroupInfo() : null;

        var colcount = 0;
        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (!(cname.startsWith('<') ||
                cname == '' ||
                (cname.buttontext && cname.buttontext.startsWith('<')))) {
                colcount++
            }
        }

        /*var $rootsScope = $('body').scope('$rootsScope');
        if (typeof ($rootsScope) != 'undefined' && $rootsScope.UserInfo.CompanyName != '') {
            rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + $rootsScope.UserInfo.CompanyName + "</th></tr>'");
        }*/

        if (groups != null) {
            groupcolumn = [];
            for (var j = 0, jlen = groups.length; j < jlen; j++) {
                var cols = [];
                for (var k = 0, klen = groups[j].length; k < klen; k++) {
                    var columns = groups[j][k].extractColumns();;
                    var alength = columns.length;
                    for (var cj = 0; cj < columns.length; cj++) {
                        var cname = columns[cj].name;
                        if (cname.startsWith('<')) {
                            alength--;
                        }
                    }
                    cols.push(groups[j][k].name);
                }
                rows.push(cols.join(','));
            }
        }

        for (var j = 0, len = grid.getColumns().length; j < len; j++) {
            var cname = grid.getColumns()[j].name;
            if (cname == '' || cname.startsWith('<') || (grid.getColumns()[j].buttontext && grid.getColumns()[j].buttontext.startsWith('<'))) {
                skipIndex.push(j);
            }
            else {
                colname.push(grid.getColumns()[j].name);
            }
        }
        rows.push(colname.join(','));

        var selectedIndexes = grid.getSelectedRows();
        var singlerow = [];
        for (var i = 0, l = grid.getData().getLength(); i < l; i++) {
            if (selectedIndexes && selectedIndexes.length > 0 &&
                selectedIndexes.indexOf(i) < 0)
                continue;
            for (var j = 0, len = grid.getColumns().length; j < len; j++) {

                if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;


                //singlerow.push(grid.getDataItem(i)[grid.getColumns()[j].field]);
                var item = grid.getDataItem(i);
                var column = grid.getColumns()[j];
                var value = item[column.field];
                // Added 
                if (typeof (column.getValue) != 'undefined')
                    value = column.getValue(item, column);
                //
                if (typeof (value) == 'undefined') {
                    if (typeof (column.getValue) != 'undefined')
                        value = column.getValue(item, column);
                }
                if (typeof (value) == 'undefined') {
                    if (typeof (column.formatevent) != 'undefined')
                        value = column.formatevent(item, column);
                }
                if (column.filtertype == 'number' || column.total || column.average) {
                    if (column.decimal && column.decimal > 0) {
                        singlerow.push(this.GetNumber(value).round(column.decimal));
                    }
                    else {
                        if (value && value != null && value.indexOf && value.indexOf('&nbsp;') >= 0) {
                            value = value.replaceAll('&nbsp;', ' ');
                        }
                        singlerow.push(value);
                    }
                }
                else if (value && value != null) {
                    fvalue = value.replaceAll('&nbsp;', ' ');
                    if (fvalue && fvalue != null && fvalue.indexOf('<') >= 0 && fvalue.indexOf('>') > 0) {
                        fvalue = this.getInnerText(fvalue);
                    }
                    singlerow.push(fvalue);
                }
                else {
                    var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
                    if (fvalue && fvalue != null && fvalue.indexOf && fvalue.indexOf('&nbsp;') >= 0) {
                        fvalue = fvalue.replaceAll('&nbsp;', ' ');
                    }
                    if (fvalue && fvalue != null && fvalue.indexOf('<') >= 0 && fvalue.indexOf('>') > 0) {
                        fvalue = this.getInnerText(fvalue);
                    }
                    singlerow.push(fvalue);

                }
            }
            rows.push(processRow(singlerow));
            singlerow = [];
        }


        if (this.enableTotal) {
            var singlerow = [];

            for (var columnIdx = 0; columnIdx < grid.getColumns().length; columnIdx++) {
                if (skipIndex.length > 0 && skipIndex.indexOf(columnIdx) != -1) continue;
                if (typeof (grid.getColumns()[columnIdx].total) != 'undefined'
                    && grid.getColumns()[columnIdx].total)
                    singlerow.push(this.UpdateTotal(columnIdx, grid, true));
                else if (typeof (grid.getColumns()[columnIdx].average) != 'undefined'
                    && grid.getColumns()[columnIdx].average)
                    singlerow.push(this.UpdateAverage(columnIdx, grid, true));
                else
                    singlerow.push('');
            }
            rows.push(singlerow.join(','));
            singlerow = [];
        }


        if (additionalrows && additionalrows != null)
            rows.push(additionalrows.join(','));


        ////for (var i = 0; i < rows.length; i++) {
        ////    csvFile += processRow(rows[i]);
        ////}

        var blob = new Blob([rows.join('\r')], { type: 'text/csv;charset=utf-8;' });
        if (navigator.msSaveBlob) { // IE 10+
            navigator.msSaveBlob(blob, "filename.csv");
        } else {
            var link = document.createElement("a");
            if (link.download !== undefined) { // feature detection
                // Browsers that support HTML5 download attribute
                var url = URL.createObjectURL(blob);
                link.setAttribute("href", url);
                link.setAttribute("download", "filename.csv");
                link.style.visibility = 'hidden';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        }

    }

    //this.ExportCSV = function (grid, title, subtitle, allrows, additionalrows) {


    //    var processRow = function (row) {
    //        var finalVal = '';
    //        for (var j = 0; j < row.length; j++) {
    //            var innerValue = row[j] == undefined || row[j] === null ? '' : row[j].toString();
    //            if (row[j] instanceof Date) {
    //                innerValue = row[j].toLocaleString();
    //            };
    //            var result = innerValue.replace(/"/g, '').replace(/,/g, '');
    //            if (j > 0)
    //                finalVal += ',';
    //            finalVal += result;
    //        }
    //        return finalVal;
    //    };

    //    var csvFile = '';
    //    var rows = [];
    //    var colname = [];
    //    var skipIndex = [];
    //    var formator = [];

    //    if (title && title != null)
    //        rows.push(title);
    //    if (subtitle && subtitle != null)
    //        rows.push(title);

    //    var groups = typeof (grid.getGroupInfo) != 'undefined' ? grid.getGroupInfo() : null;

    //    var colcount = 0;
    //    for (var j = 0, len = grid.getColumns().length; j < len; j++) {
    //        var cname = grid.getColumns()[j].name;
    //        if (!cname.startsWith('<')) {
    //            colcount++
    //        }
    //    }

    //    /*var $rootsScope = $('body').scope('$rootsScope');
    //    if (typeof ($rootsScope) != 'undefined' && $rootsScope.UserInfo.CompanyName != '') {
    //        rows.push("<Tr><th style = 'text-align:center' colspan='" + colcount + "'>" + $rootsScope.UserInfo.CompanyName + "</th></tr>'");
    //    }*/

    //    if (groups != null) {
    //        groupcolumn = [];
    //        for (var j = 0, jlen = groups.length; j < jlen; j++) {
    //            var cols = [];
    //            for (var k = 0, klen = groups[j].length; k < klen; k++) {
    //                var columns = groups[j][k].extractColumns();;
    //                var alength = columns.length;
    //                for (var cj = 0; cj < columns.length; cj++) {
    //                    var cname = columns[cj].name;
    //                    if (cname.startsWith('<')) {
    //                        alength--;
    //                    }
    //                }
    //                cols.push(groups[j][k].name);
    //            }
    //            rows.push(cols.join(','));
    //        }
    //    }

    //    for (var j = 0, len = grid.getColumns().length; j < len; j++) {
    //        var cname = grid.getColumns()[j].name;
    //        if (cname.startsWith('<')) {
    //            skipIndex.push(j);
    //        }
    //        else {
    //            colname.push(grid.getColumns()[j].name);
    //        }
    //    }
    //    rows.push(colname.join(','));

    //    var selectedIndexes = grid.getSelectedRows();
    //    var singlerow = [];
    //    for (var i = 0, l = grid.getData().getLength(); i < l; i++) {
    //        if (selectedIndexes && selectedIndexes.length > 0 &&
    //            selectedIndexes.indexOf(i) < 0)
    //            continue;


    //        for (var j = 0, len = grid.getColumns().length; j < len; j++) {

    //            if (skipIndex.length > 0 && skipIndex.indexOf(j) != -1) continue;


    //            //singlerow.push(grid.getDataItem(i)[grid.getColumns()[j].field]);
    //            var item = grid.getDataItem(i);
    //            var column = grid.getColumns()[j];
    //            var value = item[column.field];
    //            if (typeof (value) == 'undefined') {
    //                if (typeof (column.getValue) != 'undefined')
    //                    value = column.getValue(item, column);
    //            }
    //            if (typeof (value) == 'undefined') {
    //                if (typeof (column.formatevent) != 'undefined')
    //                    value = column.formatevent(item, column);
    //            }
    //            if (column.filtertype == 'number' || column.total || column.average) {
    //                if (column.decimal && column.decimal > 0) {
    //                    singlerow.push(this.GetNumber(value).round(column.decimal));
    //                }
    //                else {
    //                    singlerow.push(value);
    //                }
    //            }
    //            else {
    //                var fvalue = grid.getFormatter(i, column)(i, j, value, column, item);
    //                singlerow.push(fvalue);
    //            }
    //        }
    //        rows.push(processRow(singlerow));
    //        singlerow = [];
    //    }

    //    if (this.enableTotal) {
    //        var singlerow = [];

    //        for (var columnIdx = 0; columnIdx < grid.getColumns().length; columnIdx++) {
    //            if (skipIndex.length > 0 && skipIndex.indexOf(columnIdx) != -1) continue;
    //            if (typeof (grid.getColumns()[columnIdx].total) != 'undefined'
    //                && grid.getColumns()[columnIdx].total)
    //                singlerow.push(this.UpdateTotal(columnIdx, grid, true));
    //            if (typeof (grid.getColumns()[columnIdx].average) != 'undefined'
    //                && grid.getColumns()[columnIdx].average)
    //                singlerow.push(this.UpdateAverage(columnIdx, grid, true));
    //        }

    //        rows.push(singlerow.join(','));
    //        singlerow = [];
    //    }

    //    if (additionalrows && additionalrows != null)
    //        rows.push(additionalrows.join(','));


    //    ////for (var i = 0; i < rows.length; i++) {
    //    ////    csvFile += processRow(rows[i]);
    //    ////}

    //    var blob = new Blob([rows.join('\r')], { type: 'text/csv;charset=utf-8;' });
    //    if (navigator.msSaveBlob) { // IE 10+
    //        navigator.msSaveBlob(blob, "filename.csv");
    //    } else {
    //        var link = document.createElement("a");
    //        if (link.download !== undefined) { // feature detection
    //            // Browsers that support HTML5 download attribute
    //            var url = URL.createObjectURL(blob);
    //            link.setAttribute("href", url);
    //            link.setAttribute("download", "filename.csv");
    //            link.style.visibility = 'hidden';
    //            document.body.appendChild(link);
    //            link.click();
    //            document.body.removeChild(link);
    //        }
    //    }

    //}

    var $rootScope = angular.element($("#" + gridcontainer)[0]).injector().invoke(function ($rootScope) { return $rootScope; })
    var $scope = angular.element($("#" + gridcontainer)[0]).scope();

    if (typeof (additonalheight) != 'undefined') {
        var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
        var iHeight = available - (100 + additonalheight);
        if (typeof (iHeight) != 'undefined' && iHeight > 50) {
            height = iHeight + 'px';
        }
        else {
            height = '50px';
        }
    }

    this.dataView = this.Ex ? new SlickEx.Data.DataView({ inlineFilters: false }) : new Slick.Data.DataView({ inlineFilters: false });
    this.dataView.setFilterArgs(this);
    this.gridDiv = $("<div class='grid' style='width:" + width + ";height:" + height + ";'></div>");

    if (enableSelector) {
        var checkboxSelector = new Slick.CheckboxSelectColumn({
            cssClass: "slick-cell-checkboxsel"
        });
        this.columns.push(checkboxSelector.getColumnDefinition());
        this.columns[0].width = 40;
    }

    this.gridDiv.appendTo($("#" + gridcontainer));
    this.gridDiv[0].grid = this;
    this.gridDiv.grid = this;


    $scope.$on("windowSizeChanged", function (evt, data) {
        //var infyGrid = $('#' + gridcontainer)[0].children[0].grid;
        ///*infyGrid.gridDiv.height(data.height);
        //infyGrid.gridDiv.width(data.width);*/

        // //var gridCondainer = $('#' + gridcontainer);
        // //gridCondainer.height(250);
        ////gridCondainer.width(400);

        //infyGrid.gridDiv.height(250);

        //infyGrid.grid.updateRowCount();
        //infyGrid.grid.render();

        ///*infyGrid.gridDiv.width(400);
        //infyGrid.gridPageDiv.width(400);*/
        //infyGrid.grid.grid.resizeCanvas();
    });

    this.filter = function (item, args) {
        for (var columnId in args.columnFilters) {
            if (columnId !== undefined && args.columnFilters[columnId] !== "") {
                var cindex = args.grid.getColumnIndex(columnId);
                var c = args.grid.getColumns()[cindex];

                if (typeof (c.commafilter) != 'undefined' && c.commafilter) {
                    if (item[c.field] == null)
                        return false;
                    var listofvalues = args.columnFilters[columnId].split(',');
                    var exist = false;
                    for (var lv = 0; !exist && lv < listofvalues.length; lv++) {
                        if (item[c.field].toString().toLowerCase().startsWith(listofvalues[lv].toLowerCase())) {
                            exist = true;
                        }
                        if (listofvalues[lv].toString().toLowerCase().startsWith('%') &&
                            item[c.field].toString().toLowerCase().indexOf(listofvalues[lv].toLowerCase().substr(1)) >= 0)
                            exist = true;
                    }
                    if (!exist)
                        return false;
                }
                else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'number') {
                    if (args.columnFilters[columnId].length > 0) {
                        var operator = args.columnFilters[columnId].charAt(0);
                        if (operator == '>' || operator == '<' || operator == '=') {
                            var d = parseFloat(args.columnFilters[columnId].substr(1));
                            switch (args.columnFilters[columnId].charAt(0)) {
                                case '>':
                                    return item[c.field] > d;
                                    break;
                                case '<':
                                    return item[c.field] < d;
                                    break;
                                case '=':
                                    return item[c.field] == d;
                                    break;
                            }
                        }
                        else {
                            var d = parseFloat(args.columnFilters[columnId]);
                            if (typeof (item[c.field]) != 'undefined') {
                                return item[c.field] == d;
                            }
                            var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                            return fvalue == d;
                        }
                        //return item[c.field] != null && item[c.field].toString().indexOf(args.columnFilters[columnId]) > 0;
                    }
                }
                else if (typeof (args.columnTypes[columnId]) != 'undefined' && args.columnTypes[columnId] == 'reference') {
                    //if (item[c.field] == null)
                    //    return false;

                    var fvalue = c.formatter(0, cindex, item[c.field], c, item);
                    if (typeof (fvalue) == 'undefined' || fvalue == null ||
                        !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                        if (!args.columnFilters[columnId].startsWith('%') ||
                            fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                            return false;
                        }
                    }
                }
                else {

                    var fvalue = typeof (item[c.field]) != 'undefined' ? item[c.field] : c.formatter(0, cindex, item[c.field], c, item);
                    if (fvalue == null ||
                        !fvalue.toString().toLowerCase().startsWith(args.columnFilters[columnId].toLowerCase())) {
                        if (!args.columnFilters[columnId].startsWith('%') ||
                            fvalue.toString().toLowerCase().indexOf(args.columnFilters[columnId].toLowerCase().substr(1)) < 0) {
                            return false;
                        }
                    }
                    //return true;
                }
            }
        }
        return true;
    }

    this.onKeyupGridHeaderRow = function () {
        var $this = $(this);

        var columnId = $this.data('columnId');
        if (columnId != null) {
            this.columnFilters[columnId] = $.trim($this.val());
            this.dataView.refresh();
        }
    }


    this.ApplyFilter = function (columnId, value) {
        this.columnFilters = [];
        this.columnFilters[columnId] = value;
        this.dataView.refresh();
    }

    this.EnableFilter = function () {
        this.dataView.setFilter(this.filter);

        this.grid.onHeaderRowCellRendered.subscribe(function (e, args) {
            if (typeof (args.column.enablefilter) != 'undefined' && args.column.enablefilter) {
                $(args.node).empty();
                $("<input type='text'>")
                    .data("columnId", args.column.id)
                    .data("column", args.column)
                    .data("source", this)
                    .val(this.infyGrid.columnFilters[args.column.id])
                    .appendTo(args.node);
            }
        });

        this.grid.headercompleted = false;
        this.grid.onHeaderRowCellRenderComplete.subscribe(function (e, args) {
            if (!args.source.headercompleted) {
                args.source.headercompleted = true;
                $(args.source.getHeaderRow()).delegate(":input", "change keyup", function (e) {
                    var columnId = $(this).data("columnId");
                    var grid = $(this).data("source");
                    var column = $(this).data("column");
                    if (columnId != null) {
                        grid.infyGrid.columnFilters[columnId] = $.trim($(this).val());
                        grid.infyGrid.columnTypes[columnId] = column.filtertype;
                        grid.infyGrid.dataView.refresh();
                    }
                });
            }
        });

    }

    this.UpdateSerial = function (data) {
        data = !data ? grid.getData() : data;
        $.each(data, function (k, v) {
            v._SERIALNO = (k + 1);
        });
    }

    this.UpdateData = function (data, objectIdProperty) {
        if (this.enableSerial) {
            var serialcolumn = this.columns.find(function (item) {
                return item.id == '_SERIALNO';
            });
            if (typeof (serialcolumn) == 'undefined' || serialcolumn == null) {
                var serialcolumn = this.AddFormatedColumn('_SERIALNO', 'S.No', 40, 'N2', 0);
                var index = this.columns.indexOf(serialcolumn);
                //$scope.columns.splice(index, 1);
                this.columns.splice(0, 0, serialcolumn);
            }
        }

        if ($(this.grid.getHeaderRow())) {
            this.columnFilters = [];
            $(this.grid.getHeaderRow()).find("input").val("");
        }

        if (this.enableSerial)
            this.UpdateSerial(data);

        if (typeof (objectIdProperty) != 'undefined') {

            //if (data.length > 0 && typeof (data[0][objectIdProperty]) == 'undefined') {
            $.each(data, function (k, v) {
                v[objectIdProperty] = k;
            })
            //}

            this.dataView.beginUpdate();
            this.dataView.setItems(data, objectIdProperty);
            this.dataView.setFilterArgs(this);
            this.dataView.setFilter(this.filter);
            this.dataView.endUpdate();
        }
        else {
            if (data.length > 0) {
                if (typeof (data[0].id) == 'undefined' || typeof (data[data.length - 1].id) == 'undefined' ||
                    (data.length > 1 && data[data.length - 2].id == data[data.length - 1].id)) {
                    for (var i = 0; i < data.length; i++)
                        data[i].id = i;
                }
            }

            this.dataView.beginUpdate();
            this.dataView.setItems(data, 'id');
            this.dataView.setFilterArgs(this);
            this.dataView.setFilter(this.filter);
            this.dataView.endUpdate();
        }
        this.grid.invalidate();
        this.grid.render();
        if (typeof (this.grid.adjustColumnWidth) != 'undefined')
            this.grid.adjustColumnWidth();

        if (this.enableTotal)
            this.UpdateAllTotals(this.grid);
    }

    this.columnCreator = function (row, cell, value, columnDef, dataContext) {
        return columnDef.formatevent(dataContext, columnDef);
    }

    this.clickevents = [];
    this.buttonClick = function (e, args) {
        args.item = this.dataView.getItem(args.row);
        if (this.columns[args.cell] &&
            this.columns[args.cell].clickevent &&
            this.columns[args.cell].clickevent(e, args)) {
            this.grid.updateRow(args.row);
        }
    }

    this.Event_Subscription_Click = false;
    this.SubscribeClickEvent = function () {
        if (this.Event_Subscription_Click) return;
        this.Event_Subscription_Click = true;
    }

    this.buttonCreator = function (row, cell, value, columnDef, dataContext) {
        if (columnDef.buttontext.startsWith('<i>'))
            return "<button type='button' class='btn btn-primary btn-xs'>" + columnDef.buttontext + '</input>';

        if (columnDef.buttontext.startsWith('<'))
            return columnDef.buttontext;

        return "<input type='button' class='btn btn-primary btn-xs' value='" + columnDef.buttontext + "'/>";
    }

    this.multiButtonCreator = function (row, cell, value, columnDef, dataContext) {
        var list = [];
        $.each(columnDef.buttontext, function (k, v) {
            list.push("<input type='button' class='btn btn-primary btn-xs' value='" + v + "'/>")
        })
        return list.join('');
    }

    this.referenceCreator = function (row, cell, searchValue, columnDef, dataContext) {
        if (typeof (columnDef.referenceList) == 'undefined' || columnDef.referenceList == null || !angular.isArray(columnDef.referenceList))
            return searchValue;

        if (typeof (searchValue) != 'undefined') {
            var found = columnDef.referenceList.find(function (arritem) {
                return arritem.id == searchValue;
            });
            if (typeof (found) != 'undefined' && found != null)
                return found.name;
        }
        return typeof (columnDef.defaultValue) == 'undefined' ? "None" : columnDef.defaultValue;
    }

    this.AddMultiButtonColumn = function (name, header, width, value, clickevent) {
        this.SubscribeClickEvent();
        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: false,
            formatter: this.multiButtonCreator, clickevent: clickevent, buttontext: value, headerCssClass: 'text-center'
        });
        return this.columns[this.columns.length - 1];
    }



    this.AddButtonColumn = function (name, header, width, value, clickevent) {
        this.SubscribeClickEvent();
        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: false,
            formatter: this.buttonCreator, clickevent: clickevent, buttontext: value, headerCssClass: 'text-center'
        });
        return this.columns[this.columns.length - 1];
    }

    this.AddSerialNo = function (width, isreverse) {
        this.columns.push({
            id: '_serialno', name: 'S.No', field: '_serialno', width: width, minWidth: width,
            resizable: true, focusable: true, selectable: true,
            formatter: function (row) {
                return row + 1;
            },
            sortable: true, headerCssClass: 'text-center'
        });
        return this.columns[this.columns.length - 1];
    }

    this.AddReferenceColumn = function (name, header, width, referenceList) {

        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: true,
            formatter: this.referenceCreator, referenceList: referenceList, sortable: true, filtertype: 'reference', enablefilter: true,
            headerCssClass: 'text-center'
        });
        var refcolumn = this.columns[this.columns.length - 1];
        //refcolumn.getValue = function (item, col) {
        //    var refitem = col.referenceList.find(function (sitem) {
        //        return sitem.id == item[item.field];
        //    });
        //    if (refitem && refitem != null) {
        //        return refitem.name;
        //    }
        //    return '';
        //};
        return refcolumn;
    }

    this.AddCustomColumn = function (name, header, width, formatter, clickevent) {

        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true,
            focusable: true, selectable: true, clickevent: clickevent,
            formatter: this.columnCreator, formatevent: formatter, headerCssClass: 'text-center'
        });

        var column = this.columns[this.columns.length - 1];
        if (formatter && typeof (formatter) == 'function') {
            column.enablefilter = true;
            column.sortable = true;
            column.getValue = formatter;
            column.filtertype = 'reference';
        }

        return column;
    }

    this.AddColumn = function (column) {
        this.columns.push(column);
        return this.columns[this.columns.length - 1];
    }

    this.AddFormatrerColumn = function (name, header, width, formater) {
        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: true, formatter: formater,
            sortable: true, headerCssClass: 'text-center'
        });
        return this.columns[this.columns.length - 1];
    }

    this.AddFormatedColumn = function (name, header, width, format, decimal) {
        if (typeof (decimal) == 'undefined')
            this.columns.push({
                id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: true, formatter: Slick.Formatters.DateFormatter,
                format: format, decimal: decimal, sortable: true, headerCssClass: 'text-center', filtertype: 'date'
            });
        else
            this.columns.push({
                id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: true, formatter: Slick.Formatters.IndianNumberFormatter,
                format: format, decimal: decimal, cssClass: "rightalign", sortable: true, headerCssClass: 'text-center', filtertype: 'number', enablefilter: true
            });
        return this.columns[this.columns.length - 1];
    }

    this.RightAlighn = function (cellNode, row, rowData, columnsObject) {
        $(cellNode).addClass('rightalign');
    };

    this.AddHiddenColumn = function (name, header, width) {
        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: false, focusable: false,
            selectable: false, sortable: false, enablefilter: false, headerCssClass: 'reallyHidden', cssClass: 'reallyHidden'
        });
        return this.columns[this.columns.length - 1];
    }

    this.AddSimpleColumn = function (name, header, width) {
        this.columns.push({
            id: name, name: header, field: name, width: width, minWidth: width, resizable: true, focusable: true, selectable: true,
            sortable: true, enablefilter: true, headerCssClass: 'text-center', filtertype: 'string'
        });
        return this.columns[this.columns.length - 1];
    }

    this.setCellCSS = function (style) {
        $.each(this.columns, function (k, v) {
            if (typeof (v.cssClass) == 'undefined' || v.cssClass == null)
                v.cssClass = style;
            else
                v.cssClass += ' ' + style;
        });
    }

    this.Initialize = function () {
        //this.grid = this.Ex ? new SlickEx.Grid(this.gridDiv, this.dataView, this.columns, this.options) : new Slick.Grid(this.gridDiv, this.dataView, this.columns, this.options);
        this.grid = new SlickEx.Grid(this.gridDiv, this.dataView, this.columns, this.options);
        this.grid.printPlugin = new Slick.Plugins.Print();
        this.grid.registerPlugin(this.grid.printPlugin);
        var tooltipcolumn = this.columns.find(function (item) {
            return typeof (item.toolTip) != 'undefined';
        });
        if (tooltipcolumn && tooltipcolumn != null)
            this.grid.registerPlugin(new Slick.AutoTooltips({ 'useToolTipForCells': true }));
        this.grid.infyGrid = this;
        this.dataView.infyGrid = this;

        if (enablePagination) {
            this.gridPageDiv = $("<div style='width:" + width + ";height:20px;'></div>");
            this.gridPageDiv.appendTo($("#" + gridcontainer));
            this.pager = new Slick.Controls.Pager(this.dataView, this.grid, this.gridPageDiv);

            this.dataView.onPagingInfoChanged.subscribe(function (e, pagingInfo) {
                this.infyGrid.onPageInfoChanged(e, pagingInfo);
            });

            this.grid.onScroll.subscribe(function (e, args) {
                this.infyGrid.onScroll(e, args);
            });

        }

        // wire up model events to drive the grid
        this.dataView.onRowCountChanged.subscribe(function (e, args) {
            this.infyGrid.grid.updateRowCount();
            this.infyGrid.grid.render();
            this.infyGrid.UpdateAllTotals(this.infyGrid.grid);
        });

        this.dataView.onRowsChanged.subscribe(function (e, args) {
            this.infyGrid.grid.invalidateRows(args.rows);
            this.infyGrid.grid.render();
            this.infyGrid.UpdateAllTotals(this.infyGrid.grid);
            this.infyGrid.onRowValueChanged(e, args);
        });

        this.grid.onCellChange.subscribe(function (e, args) {
            if (args.grid.getColumns()[args.cell].total) {
                this.infyGrid.UpdateAllTotals(this.infyGrid.grid);
            }
            this.infyGrid.onCellChangedChanged(e, args);
        });

        this.grid.onDblClick.subscribe(function (e, args) {
            this.infyGrid.onDblClick(e, args);
        });

        //this.grid.onSort.subscribe(function (e, args) {
        //    var cols = args.sortCols;
        //    var rows = this.infyGrid.dataView.getItems();
        //    rows.sort(function (dataRow1, dataRow2) {
        //        for (var i = 0, l = cols.length; i < l; i++) {
        //            var field = cols[i].sortCol.field;
        //            var sign = cols[i].sortAsc ? 1 : -1;
        //            var value1 = dataRow1[field], value2 = dataRow2[field];
        //            var result = (value1 == value2 ? 0 : (value1 > value2 ? 1 : -1)) * sign;
        //            if (result != 0) {
        //                return result;
        //            }
        //        }
        //        return 0;
        //    });
        //    this.infyGrid.dataView.refresh();
        //    this.infyGrid.grid.invalidate();
        //    this.infyGrid.grid.render();
        //});

        this.sortFunction = function (e, args) {
            var grid = args.grid;
            var data = args.grid.getData();
            var cols = args.sortCols;

            var sortstring = false;
            if (typeof (cols[0].sortCol.getValue) != 'undefined' &&
                data.getLength() > 0) {
                var value1 = cols[0].sortCol.getValue(data.getItem(0), cols[0].sortCol);
                var value2 = cols[0].sortCol.getValue(data.getItem(data.getLength() - 1), cols[0].sortCol);
                if (typeof (value1) == 'string' ||
                    typeof (value2) == 'string')
                    sortstring = true;
            }
            else {
                if (data.getLength() > 0) {
                    if (typeof (data.getItem(0)[cols[0].sortCol.field]) == 'string' ||
                        typeof (data.getItem(data.getLength() - 1)[cols[0].sortCol.field]) == 'string')
                        sortstring = true;
                }
            }

            var strcomparer = function (a, b) {
                var value1 = a[args.sortCols[0].sortCol.field];
                var value2 = b[args.sortCols[0].sortCol.field];
                if (value1 && value1 != null) value1 = value1.toLowerCase();
                if (value2 && value2 != null) value2 = value2.toLowerCase();
                return (value1 > value2) ? 1 : -1;
            }

            var comparer = function (a, b) {
                return (a[args.sortCols[0].sortCol.field] > b[args.sortCols[0].sortCol.field]) ? 1 : -1;
            }

            var comparerfromgetvalue = function (a, b) {
                return (args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol) >
                    args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol)) ? 1 : -1;
            }

            var strfunctioncomparer = function (a, b) {
                var value1 = args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol);
                var value2 = args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol);
                if (value1 && value1 != null) value1 = value1.toLowerCase();
                if (value2 && value2 != null) value2 = value2.toLowerCase();
                return value1 > value2 ? 1 : -1;
            }

            var functioncomparer = function (a, b) {
                return args.sortCols[0].sortCol.getValue(a, args.sortCols[0].sortCol) > args.sortCols[0].sortCol.getValue(b, args.sortCols[0].sortCol) ? 1 : -1;
            }

            var formattercomparer = function (a, b) {
                var avalue = args.sortCols[0].sortCol.formatter(0, 0, a[args.sortCols[0].sortCol.field], args.sortCols[0].sortCol, a);
                var bvalue = args.sortCols[0].sortCol.formatter(0, 0, b[args.sortCols[0].sortCol.field], args.sortCols[0].sortCol, b);
                return avalue > bvalue ? 1 : -1;
            }

            if (args.sortCols[0].sortCol.filtertype == 'number' &&
                data.getLength() > 0) {
                if (typeof (data.getItem(0)[args.sortCols[0].sortCol.field]) != 'undefined')
                    data.sort(comparer, args.sortCols[0].sortAsc);
                else
                    data.sort(functioncomparer, args.sortCols[0].sortAsc);
            }
            else if (cols[0].sortCol.formatter != undefined && cols[0].sortCol.formatter != null) {
                data.sort(formattercomparer, args.sortCols[0].sortAsc);
            }
            else if (typeof (cols[0].sortCol.getValue) != 'undefined') {

                data.sort(sortstring ? strfunctioncomparer : functioncomparer, args.sortCols[0].sortAsc);
            }
            else {
                data.sort(sortstring ? strcomparer : comparer, args.sortCols[0].sortAsc);
            }

            grid.invalidate();
            grid.render();
        };

        this.grid.onSort.subscribe(this.sortFunction);

        //this.grid.onSort.subscribe(function (e, args) {
        //    var grid = args.grid;
        //    var data = args.grid.getData();
        //    var cols = args.sortCols;
        //    if (typeof (cols[0].sortCol.getValue) != 'undefined') {

        //        var sortstring = false;
        //        if (data.getLength()>0) {
        //            var value1 = cols[0].sortCol.getValue(data.getItem(0), cols[0].sortCol);
        //            var value2 = cols[0].sortCol.getValue(data.getItem(data.getLength()-1), cols[0].sortCol);
        //            if (typeof (value1) == 'string' ||
        //                typeof (value2) == 'string')
        //                sortstring = true;
        //        }

        //        data.sort(function (dataRow1, dataRow2) {
        //            for (var i = 0, l = cols.length; i < l; i++) {
        //                var field = cols[i].sortCol.field;
        //                var sign = cols[i].sortAsc ? 1 : -1;
        //                var value1 = cols[i].sortCol.getValue(dataRow1, cols[i].sortCol), value2 = cols[i].sortCol.getValue(dataRow2, cols[i].sortCol);

        //                if (sortstring) {
        //                    if (value1 != null && typeof (value1) == 'string')
        //                        value1 = value1.toUpperCase();
        //                    if (value2 != null && typeof (value2) == 'string')
        //                        value2 != value2.toUpperCase();
        //                }

        //                var result = (value1 == value2 ? 0 : (value1 > value2 ? 1 : -1)) * sign;
        //                if (result != 0) {
        //                    return result;
        //                }
        //            }
        //            return 0;
        //        });
        //    }
        //    else {

        //        var sortstring = false;
        //        if (data.getLength() > 0) {
        //            if (typeof (data.getItem(0)[cols[0].sortCol.field]) == 'string' ||
        //                typeof (data.getItem(data.getLength() - 1)[cols[0].sortCol.field]) == 'string')
        //                sortstring = true;
        //        }
        //        data.sort(function (dataRow1, dataRow2) {
        //            for (var i = 0, l = cols.length; i < l; i++) {
        //                var field = cols[i].sortCol.field;
        //                var sign = cols[i].sortAsc ? 1 : -1;
        //                var value1 = dataRow1[field], value2 = dataRow2[field];

        //                if (sortstring) {
        //                    if (value1 != null && typeof (value1) == 'string')
        //                        value1 = value1.toUpperCase();
        //                    if (value2 != null && typeof (value2) == 'string')
        //                        value2 != value2.toUpperCase();
        //                }

        //                var result = (value1 == value2 ? 0 : (value1 > value2 ? 1 : -1)) * sign;
        //                if (result != 0) {
        //                    return result;
        //                }
        //            }
        //            return 0;
        //        });
        //    }
        //    grid.invalidate();
        //    grid.render();
        //});

        this.referesh = function () {
            this.dataView.refresh();
            this.grid.invalidate();
            this.grid.render();
        }

        if (this.Event_Subscription_Click) {
            this.grid.onClick.subscribe(function (e, args) {
                if ($(e.target).hasClass("btn")) {
                    var item = args.grid.infyGrid.buttonClick(e, args);
                }
            });
        }

        //this.copyManager = new Slick.CellCopyManager();
        //this.grid.registerPlugin(this.copyManager);

        var undoRedoBuffer = {
            commandQueue: [],
            commandCtr: 0,

            queueAndExecuteCommand: function (editCommand) {
                this.commandQueue[this.commandCtr] = editCommand;
                this.commandCtr++;
                editCommand.execute();
            },

            undo: function () {
                if (this.commandCtr == 0) { return; }

                this.commandCtr--;
                var command = this.commandQueue[this.commandCtr];

                if (command && Slick.GlobalEditorLock.cancelCurrentEdit()) {
                    command.undo();
                }
            },
            redo: function () {
                if (this.commandCtr >= this.commandQueue.length) { return; }
                var command = this.commandQueue[this.commandCtr];
                this.commandCtr++;
                if (command && Slick.GlobalEditorLock.cancelCurrentEdit()) {
                    command.execute();
                }
            }
        }

        var pluginOptions = {
            clipboardCommandHandler: function (editCommand) { undoRedoBuffer.queueAndExecuteCommand.call(undoRedoBuffer, editCommand); },
            readOnlyMode: false,
            includeHeaderWhenCopying: false,
            newRowCreator: function (count) {
                for (var i = 0; i < count; i++) {
                    var item = {
                        id: "newRow_" + newRowIds++
                    }
                    grid.getData().addItem(item);
                }
            }
        };

        // set keyboard focus on the grid

        if (this.options.showHeaderRow) {
            this.EnableFilter();
        }
        this.grid.init();

        if (enableSelector) {
            var rowselectionmodel = new Slick.RowSelectionModel({ selectActiveRow: false });
            this.grid.setSelectionModel(rowselectionmodel);
            this.grid.registerPlugin(checkboxSelector);
            this.dataView.syncGridSelection(this.grid, true);
            rowselectionmodel.infyGrid = this;
            rowselectionmodel.onSelectedRangesChanged.subscribe(function (eventdata, range) {
                eventdata.source.infyGrid.onSelectedRangesChanged(range, eventdata.source);
            });
        }
        else {
            this.grid.setSelectionModel(new Slick.CellSelectionModel());
        }

        this.grid.getCanvasNode().focus();
        this.grid.registerPlugin(new Slick.CellExternalCopyManager(pluginOptions));
    }

    this.onSelectedRangesChanged = function (range, rowmodel) {

    };

    this.onCellChangedChanged = function (e, args) {

    };

    this.onDblClick = function (e, args) {

    };


    this.SetSelectedRows = function (selectedRows) {
        this.grid.setSelectedRows(selectedRows);
    }

    this.destroy = function () {
        this.grid.destroy();
        $("#" + gridcontainer).empty();
    }

    this.GetSelectedRows = function () {
        var selectedData = [], selectedIndexes;
        selectedIndexes = this.grid.getSelectedRows();
        for (var index = 0; index < selectedIndexes.length; index++) {
            var value = this.dataView.getItem(selectedIndexes[index]);
            if (typeof (value) != 'undefined' && value != null)
                selectedData.push(value);
        }
        return selectedData;
    }


    function clamp_range(range) {
        if (range.e.r >= (1 << 20)) range.e.r = (1 << 20) - 1;
        if (range.e.c >= (1 << 14)) range.e.c = (1 << 14) - 1;
        return range;
    }

    var crefregex = /(^|[^._A-Z0-9])([$]?)([A-Z]{1,2}|[A-W][A-Z]{2}|X[A-E][A-Z]|XF[A-D])([$]?)([1-9]\d{0,5}|10[0-3]\d{4}|104[0-7]\d{3}|1048[0-4]\d{2}|10485[0-6]\d|104857[0-6])(?![_.\(A-Za-z0-9])/g;

    /*
        inserts `nrows` rows BEFORE specified `start_row`
        - ws         = worksheet object
        - start_row  = starting row (0-indexed) | default 0
        - nrows      = number of rows to add    | default 1
        - opts       = options:
          + fill     = set to true to "fill" cell styles and formulae
    */

    function insert_rows(ws, start_row, nrows, opts) {
        if (!ws) throw new Error("operation expects a worksheet");
        var dense = Array.isArray(ws);
        if (!nrows) nrows = 1;
        if (!start_row) start_row = 0;
        if (!opts) opts = {};

        /* extract original range */
        var range = XLSX.utils.decode_range(ws["!ref"]);
        var R = 0, C = 0;

        var formula_cb = function ($0, $1, $2, $3, $4, $5) {
            var _R = XLSX.utils.decode_row($5), _C = XLSX.utils.decode_col($3);
            if (!opts.fill ? (_R >= start_row) : (R >= start_row)) _R += nrows;
            return $1 + ($2 == "$" ? $2 + $3 : XLSX.utils.encode_col(_C)) + ($4 == "$" ? $4 + $5 : XLSX.utils.encode_row(_R));
        };

        var addr, naddr, newcell;
        /* move cells and update formulae */
        if (dense) {
            /* cells after the insert */
            for (R = range.e.r; R >= start_row; --R) {
                if (ws[R]) ws[R].forEach(function (cell) { if (cell.f) cell.f = cell.f.replace(crefregex, formula_cb); });
                ws[R + nrows] = ws[R];
            }

            /* TODO: dense mode; newly created space */
            for (R = start_row; R < start_row + nrows; ++R) ws[R] = [];

            /* cells before insert */
            for (R = 0; R < start_row; ++R) {
                if (ws[R]) ws[R].forEach(function (cell) { if (cell.f) cell.f = cell.f.replace(crefregex, formula_cb); });
            }
            range.e.r += nrows;
        } else {
            /* cells after the insert */
            for (R = range.e.r; R >= start_row; --R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    naddr = XLSX.utils.encode_cell({ r: R + nrows, c: C });
                    if (!ws[addr]) { delete ws[naddr]; continue; }
                    if (opts.fill && (ws[addr].s || ws[addr].f)) {
                        newcell = {};
                        if (ws[addr].f) { newcell.f = ws[addr].f; newcell.t = ws[addr].t; }
                        else { newcell.t = "z"; }
                    }
                    if (ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                    ws[naddr] = ws[addr];
                    if (opts.fill) { ws[addr] = newcell; console.log(ws[addr], newcell); }
                    if (range.e.r < R + nrows) range.e.r = R + nrows;
                }
            }

            /* newly created space */
            if (!opts.fill) for (R = start_row; R < start_row + nrows; ++R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    delete ws[addr];
                }
            }

            /* cells before insert */
            for (R = 0; R < start_row; ++R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    if (ws[addr] && ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                }
            }
        }

        /* write new range */
        ws["!ref"] = XLSX.utils.encode_range(clamp_range(range));

        /* merge cells */
        if (ws["!merges"]) ws["!merges"].forEach(function (merge, idx) {
            var mergerange;
            switch (typeof merge) {
                case 'string': mergerange = XLSX.utils.decode_range(merge); break;
                case 'object': mergerange = merge; break;
                default: throw new Error("Unexpected merge ref " + merge);
            }
            if (mergerange.s.r >= start_row) mergerange.s.r += nrows;
            if (mergerange.e.r >= start_row) mergerange.e.r += nrows;
            clamp_range(mergerange);
            ws["!merges"][idx] = mergerange;
        });

        /* rows */
        var rowload = [start_row, 0];
        for (R = 0; R < nrows; ++R) rowload.push(void 0);
        if (ws["!rows"]) ws["!rows"].splice.apply(ws["!rows"], rowload);
    }

    /*
        deletes `nrows` rows STARTING WITH `start_row`
        - ws         = worksheet object
        - start_row  = starting row (0-indexed) | default 0
        - nrows      = number of rows to delete | default 1
    */

    function delete_rows(ws, start_row, nrows) {
        if (!ws) throw new Error("operation expects a worksheet");
        var dense = Array.isArray(ws);
        if (!nrows) nrows = 1;
        if (!start_row) start_row = 0;

        /* extract original range */
        var range = XLSX.utils.decode_range(ws["!ref"]);
        var R = 0, C = 0;

        var formula_cb = function ($0, $1, $2, $3, $4, $5) {
            var _R = XLSX.utils.decode_row($5), _C = XLSX.utils.decode_col($3);
            if (_R >= start_row) {
                _R -= nrows;
                if (_R < start_row) return "#REF!";
            }
            return $1 + ($2 == "$" ? $2 + $3 : XLSX.utils.encode_col(_C)) + ($4 == "$" ? $4 + $5 : XLSX.utils.encode_row(_R));
        };

        var addr, naddr;
        /* move cells and update formulae */
        if (dense) {
            for (R = start_row + nrows; R <= range.e.r; ++R) {
                if (ws[R]) ws[R].forEach(function (cell) { cell.f = cell.f.replace(crefregex, formula_cb); });
                ws[R - nrows] = ws[R];
            }
            ws.length -= nrows;
            for (R = 0; R < start_row; ++R) {
                if (ws[R]) ws[R].forEach(function (cell) { cell.f = cell.f.replace(crefregex, formula_cb); });
            }
        } else {
            for (R = start_row + nrows; R <= range.e.r; ++R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    naddr = XLSX.utils.encode_cell({ r: R - nrows, c: C });
                    if (!ws[addr]) { delete ws[naddr]; continue; }
                    if (ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                    ws[naddr] = ws[addr];
                }
            }
            for (R = range.e.r; R > range.e.r - nrows; --R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    delete ws[addr];
                }
            }
            for (R = 0; R < start_row; ++R) {
                for (C = range.s.c; C <= range.e.c; ++C) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    if (ws[addr] && ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                }
            }
        }

        /* write new range */
        range.e.r -= nrows;
        if (range.e.r < range.s.r) range.e.r = range.s.r;
        ws["!ref"] = XLSX.utils.encode_range(clamp_range(range));

        /* merge cells */
        if (ws["!merges"]) ws["!merges"].forEach(function (merge, idx) {
            var mergerange;
            switch (typeof merge) {
                case 'string': mergerange = XLSX.utils.decode_range(merge); break;
                case 'object': mergerange = merge; break;
                default: throw new Error("Unexpected merge ref " + merge);
            }
            if (mergerange.s.r >= start_row) {
                mergerange.s.r = Math.max(mergerange.s.r - nrows, start_row);
                if (mergerange.e.r < start_row + nrows) { delete ws["!merges"][idx]; return; }
            } else if (mergerange.e.r >= start_row) mergerange.e.r = Math.max(mergerange.e.r - nrows, start_row);
            clamp_range(mergerange);
            ws["!merges"][idx] = mergerange;
        });
        if (ws["!merges"]) ws["!merges"] = ws["!merges"].filter(function (x) { return !!x; });

        /* rows */
        if (ws["!rows"]) ws["!rows"].splice(start_row, nrows);
    }

    /*
        inserts `ncols` cols BEFORE specified `start_col`
        - ws         = worksheet object
        - start_col  = starting col (0-indexed) | default 0
        - ncols      = number of cols to add    | default 1
        - opts       = options:
          + fill     = set to true to "fill" cell styles and formulae
    */

    function insert_cols(ws, start_col, ncols, opts) {
        if (!ws) throw new Error("operation expects a worksheet");
        var dense = Array.isArray(ws);
        if (!ncols) ncols = 1;
        if (!start_col) start_col = 0;
        if (!opts) opts = {};

        /* extract original range */
        var range = XLSX.utils.decode_range(ws["!ref"]);
        var R = 0, C = 0;

        var formula_cb = function ($0, $1, $2, $3, $4, $5) {
            var _R = XLSX.utils.decode_row($5), _C = XLSX.utils.decode_col($3);
            if (!opts.fill ? (_C >= start_col) : (C >= start_col)) _C += ncols;
            return $1 + ($2 == "$" ? $2 + $3 : XLSX.utils.encode_col(_C)) + ($4 == "$" ? $4 + $5 : XLSX.utils.encode_row(_R));
        };

        var addr, naddr, newcell;
        /* move cells and update formulae */
        if (dense) {
            for (R = range.s.r; R <= range.e.r; ++R) {
                if (!ws[R]) continue;
                /* cells before insert insert */
                for (C = 0; C < start_col; ++C) {
                    if (ws[R][C] && ws[R][C].f) ws[R][C].f = ws[R][C].f.replace(crefregex, formula_cb);
                }
                /* cells after insert */
                for (C = range.e.c; C >= start_col; --C) {
                    if (!ws[R][C]) { delete ws[R][C + ncols]; continue; }
                    if (ws[R][C] && ws[R][C].f) ws[R][C].f = ws[R][C].f.replace(crefregex, formula_cb);
                    ws[R][C + ncols] = ws[R][C];
                    delete ws[R][C];
                }
            }
            range.e.c += ncols;
        } else {
            /* cells after the insert */
            for (C = range.e.c; C >= start_col; --C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    naddr = XLSX.utils.encode_cell({ r: R, c: C + ncols });
                    if (!ws[addr]) { delete ws[naddr]; continue; }
                    if (opts.fill && (ws[addr].s || ws[addr].f)) {
                        newcell = {};
                        if (ws[addr].f) { newcell.f = ws[addr].f; newcell.t = ws[addr].t; }
                        else { newcell.t = "z"; }
                    }
                    if (ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                    ws[naddr] = ws[addr];
                    if (opts.fill) { ws[addr] = newcell; console.log(ws[addr], newcell); }
                    if (range.e.c < C + ncols) range.e.c = C + ncols;
                }
            }

            /* newly created space */
            if (!opts.fill) for (C = start_col; C < start_col + ncols; ++C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    delete ws[addr];
                }
            }

            /* cells before insert */
            for (C = 0; C < start_col; ++C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    if (ws[addr] && ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                }
            }
        }

        /* write new range */
        ws["!ref"] = XLSX.utils.encode_range(clamp_range(range));

        /* merge cells */
        if (ws["!merges"]) ws["!merges"].forEach(function (merge, idx) {
            var mergerange;
            switch (typeof merge) {
                case 'string': mergerange = XLSX.utils.decode_range(merge); break;
                case 'object': mergerange = merge; break;
                default: throw new Error("Unexpected merge ref " + merge);
            }
            if (mergerange.s.c >= start_col) mergerange.s.c += ncols;
            if (mergerange.e.c >= start_col) mergerange.e.c += ncols;
            clamp_range(mergerange);
            ws["!merges"][idx] = mergerange;
        });

        /* cols */
        var colload = [start_col, 0];
        for (C = 0; C < ncols; ++C) colload.push(void 0);
        if (ws["!cols"]) ws["!cols"].splice.apply(ws["!cols"], colload);
    }

    /*
        deletes `ncols` cols STARTING WITH `start_col`
        - ws         = worksheet object
        - start_col  = starting col (0-indexed) | default 0
        - ncols      = number of cols to delete | default 1
    */

    function delete_cols(ws, start_col, ncols) {
        if (!ws) throw new Error("operation expects a worksheet");
        var dense = Array.isArray(ws);
        if (!ncols) ncols = 1;
        if (!start_col) start_col = 0;

        /* extract original range */
        var range = XLSX.utils.decode_range(ws["!ref"]);
        var R = 0, C = 0;

        var formula_cb = function ($0, $1, $2, $3, $4, $5) {
            var _R = XLSX.utils.decode_row($5), _C = XLSX.utils.decode_col($3);
            if (_C >= start_col) {
                _C -= ncols;
                if (_C < start_col) return "#REF!";
            }
            return $1 + ($2 == "$" ? $2 + $3 : XLSX.utils.encode_col(_C)) + ($4 == "$" ? $4 + $5 : XLSX.utils.encode_row(_R));
        };

        var addr, naddr;
        /* move cells and update formulae */
        if (dense) {
        } else {
            for (C = start_col + ncols; C <= range.e.c; ++C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    naddr = XLSX.utils.encode_cell({ r: R, c: C - ncols });
                    if (!ws[addr]) { delete ws[naddr]; continue; }
                    if (ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                    ws[naddr] = ws[addr];
                }
            }
            for (C = range.e.c; C > range.e.c - ncols; --C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    delete ws[addr];
                }
            }
            for (C = 0; C < start_col; ++C) {
                for (R = range.s.r; R <= range.e.r; ++R) {
                    addr = XLSX.utils.encode_cell({ r: R, c: C });
                    if (ws[addr] && ws[addr].f) ws[addr].f = ws[addr].f.replace(crefregex, formula_cb);
                }
            }
        }

        /* write new range */
        range.e.c -= ncols;
        if (range.e.c < range.s.c) range.e.c = range.s.c;
        ws["!ref"] = XLSX.utils.encode_range(clamp_range(range));

        /* merge cells */
        if (ws["!merges"]) ws["!merges"].forEach(function (merge, idx) {
            var mergerange;
            switch (typeof merge) {
                case 'string': mergerange = XLSX.utils.decode_range(merge); break;
                case 'object': mergerange = merge; break;
                default: throw new Error("Unexpected merge ref " + merge);
            }
            if (mergerange.s.c >= start_col) {
                mergerange.s.c = Math.max(mergerange.s.c - ncols, start_col);
                if (mergerange.e.c < start_col + ncols) { delete ws["!merges"][idx]; return; }
                mergerange.e.c -= ncols;
                if (mergerange.e.c < mergerange.s.c) { delete ws["!merges"][idx]; return; }
            } else if (mergerange.e.c >= start_col) mergerange.e.c = Math.max(mergerange.e.c - ncols, start_col);
            clamp_range(mergerange);
            ws["!merges"][idx] = mergerange;
        });
        if (ws["!merges"]) ws["!merges"] = ws["!merges"].filter(function (x) { return !!x; });

        /* cols */
        if (ws["!cols"]) ws["!cols"].splice(start_col, ncols);
    }
}






