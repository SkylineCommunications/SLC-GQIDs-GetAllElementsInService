/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

24/01/2024	1.0.0.1		JYE, Skyline	Initial version
****************************************************************************
*/

namespace GetAllElementsInService_1
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a data source for retrieving all elements in a service.
    /// Implements the IGQIDataSource, IGQIOnInit, and IGQIInputArguments interfaces.
    /// </summary>
    /// <remarks>
    /// This class is used to interact with the DataMiner System (DMS) to retrieve service and element information.
    /// It provides methods for retrieving columns, getting the next page of data, initializing the data source,
    /// retrieving input arguments, and processing provided arguments.
    /// </remarks>
    [GQIMetaData(Name = "GetAllElementsInService")]
    public class MyDataSource : IGQIDataSource, IGQIOnInit, IGQIInputArguments
    {
        /// <summary>
        /// Represents the service name argument required for the data source.
        /// </summary>
        /// <remarks>
        /// This argument is used to specify the name of the service from which the elements are to be retrieved.
        /// It is a required argument for the data source.
        /// </remarks>
        private readonly GQIStringArgument _serviceNameArgument = new GQIStringArgument("Service") { IsRequired = true };

        /// <summary>
        /// Represents the DataMiner System (DMS) used for sending messages and retrieving service and element information.
        /// </summary>
        private GQIDMS _dms;

        /// <summary>
        /// Represents the name of the service for which the elements are being retrieved.
        /// </summary>
        /// <remarks>
        /// This field is populated with the value provided by the user through the GQIStringArgument '_serviceNameArgument'.
        /// It is used in the 'GetNextPage' method to send a 'GetServiceByNameMessage' to the DataMiner System (DMS).
        /// </remarks>
        private string _serviceName;

        /// <summary>
        /// Retrieves the columns for the data source.
        /// </summary>
        /// <remarks>These are the columns the user will see on the low code app.</remarks>
        /// <returns>
        /// An array of GQIColumn objects representing the columns of the data source.
        /// </returns>
        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Element Name"),
            };
        }

        /// <summary>
        /// Retrieves the next page of data from the data source.
        /// </summary>
        /// <param name="args">The arguments required to get the next page.</param>
        /// <returns>
        /// A GQIPage object containing the next page of data. The 'HasNextPage' property of the returned object indicates whether there are more pages to retrieve.
        /// </returns>
        /// <remarks>
        /// This method sends a 'GetServiceByNameMessage' to the DataMiner System (DMS) to get the service information.
        /// It then iterates over the elements in the service, sending a 'GetElementByIDMessage' for each element to get its information.
        /// The element information is added as a row to the GQIPage object that is returned.
        /// </remarks>
        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var rows = new List<GQIRow>();
            var message = new GetServiceByNameMessage(_serviceName);

            try
            {
                if (!(_dms.SendMessage(message) is ServiceInfoEventMessage service))
                    return new GQIPage(rows.ToArray()) { HasNextPage = false };

                var elementInfo = service.ServiceParams;

                foreach (var info in elementInfo)
                {
                    var dmaId = info.DataMinerID;
                    var elementId = info.ElementID;
                    var elementMessage = new GetElementByIDMessage(dmaId, elementId);

                    if (_dms.SendMessage(elementMessage) is ElementInfoEventMessage element)
                    {
                        var row = new GQIRow(
                            new[]
                            {
                                new GQICell {Value = element.Name},
                            });

                        rows.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                // Something went wrong.
            }

            return new GQIPage(rows.ToArray()) { HasNextPage = false };
        }

        /// <summary>
        /// Initializes the data source with the provided arguments.
        /// </summary>
        /// <remarks>The first method that executes in the script.</remarks>
        /// <param name="args">The initialization arguments.</param>
        /// <returns>Returns an instance of OnInitOutputArgs after initialization.</returns>
        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return new OnInitOutputArgs();
        }

        /// <summary>
        /// Retrieves the input arguments required for the data source.
        /// </summary>
        /// <remarks>
        /// These are the arguments that the user will need to provide for the data source.
        /// </remarks>
        /// <returns>
        /// An array of GQIArgument objects representing the input arguments for the data source.
        /// </returns>
        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { _serviceNameArgument };
        }

        /// <summary>
        /// Processes the provided arguments after they have been inputted.
        /// </summary>
        /// <param name="args">The arguments that have been processed.</param>
        /// <returns>Returns an instance of OnArgumentsProcessedOutputArgs after processing the arguments.</returns>
        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _serviceName = args.GetArgumentValue(_serviceNameArgument);
            return new OnArgumentsProcessedOutputArgs();
        }
    }
}