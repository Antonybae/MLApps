using System;
using System.Collections.Generic;
using System.Text;
using static MLTest.Program;

namespace MLTest
{
	static class TestPricing
	{
		internal static readonly PriceData videocardPrice = new PriceData
		{
			manufacturer = "NVIDIA",
			bitcoinCource = 6485,
			model = "GTX1060",
			price = 0	//predict it
		};
	}
}
