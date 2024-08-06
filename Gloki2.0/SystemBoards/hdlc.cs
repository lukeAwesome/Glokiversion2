using System;

public enum yahdlc_frame_t
{
	YAHDLC_FRAME_DATA,
	YAHDLC_FRAME_ACK,
	YAHDLC_FRAME_NACK,
}

public struct yahdlc_control_t
{
	public yahdlc_frame_t frame;
	public int seq_no;
}
public unsafe struct Yahdlc_state_t
{
	public int control_escape;
	public int fcs;
	public int start_index;
	public int end_index;
	public int src_index;
	public int dest_index;
	public int AdditionalFlagSequenceDetectedFlag;
	public int EndSeqAdditionalFlagSequenceDetectedFlag;
	public int MaxDataBufferSize;
	public int StationAddress;
}

namespace Gloki2._0.SystemBoards
{
	public partial class hdlc
	{
		public const int ENOMSG = 100;
		public const int EIO = 200;
		/** FCS initialization value. */
		public const int FCS16_INIT_VALUE = 0xFFFF;

		/** FCS value for valid frames. */
		public const Int32 FCS16_GOOD_VALUE = 0xF0B8;
		// HDLC Control field bit positions
		public const int YAHDLC_CONTROL_S_OR_U_FRAME_BIT = 0;
		public const int YAHDLC_CONTROL_SEND_SEQ_NO_BIT = 1;
		public const int YAHDLC_CONTROL_S_FRAME_TYPE_BIT = 2;
		public const int YAHDLC_CONTROL_POLL_BIT = 4;
		public const int YAHDLC_CONTROL_RECV_SEQ_NO_BIT = 5;

		// HDLC Control type definitions
		public const int YAHDLC_CONTROL_TYPE_RECEIVE_READY = 0;
		public const int YAHDLC_CONTROL_TYPE_RECEIVE_NOT_READY = 1;
		public const int YAHDLC_CONTROL_TYPE_REJECT = 2;
		public const int YAHDLC_CONTROL_TYPE_SELECTIVE_REJECT = 3;
		/** HDLC start/end flag sequence */
		public const int YAHDLC_FLAG_SEQUENCE = 0x7E;
		public const int YAHDLC_START_FLAG_SEQUENCE = 0x02;
		public const int YAHDLC_END_FLAG_SEQUENCE = 0x03;

		/** HDLC control escape value */
		public const int YAHDLC_CONTROL_ESCAPE = 0x7D;

		/** HDLC all station address */
		public const int YAHDLC_ALL_STATION_ADDR = 0xFF;
		public const int YAHDLC_JSON_STATION_ADDR = 0xFE;

		/** Supported HDLC frame types */

		/** Control field information */

		public void Init()
		{
		}

		public Yahdlc_state_t yahdlc_state;
		public Yahdlc_state_t yahdlc_stateRx;
		public yahdlc_control_t yahdlc_controlRx;

		public int[] fcstab =
		{ 0x0000, 0x1189, 0x2312, 0x329b,
		0x4624, 0x57ad, 0x6536, 0x74bf, 0x8c48, 0x9dc1, 0xaf5a, 0xbed3, 0xca6c,
		0xdbe5, 0xe97e, 0xf8f7, 0x1081, 0x0108, 0x3393, 0x221a, 0x56a5, 0x472c,
		0x75b7, 0x643e, 0x9cc9, 0x8d40, 0xbfdb, 0xae52, 0xdaed, 0xcb64, 0xf9ff,
		0xe876, 0x2102, 0x308b, 0x0210, 0x1399, 0x6726, 0x76af, 0x4434, 0x55bd,
		0xad4a, 0xbcc3, 0x8e58, 0x9fd1, 0xeb6e, 0xfae7, 0xc87c, 0xd9f5, 0x3183,
		0x200a, 0x1291, 0x0318, 0x77a7, 0x662e, 0x54b5, 0x453c, 0xbdcb, 0xac42,
		0x9ed9, 0x8f50, 0xfbef, 0xea66, 0xd8fd, 0xc974, 0x4204, 0x538d, 0x6116,
		0x709f, 0x0420, 0x15a9, 0x2732, 0x36bb, 0xce4c, 0xdfc5, 0xed5e, 0xfcd7,
		0x8868, 0x99e1, 0xab7a, 0xbaf3, 0x5285, 0x430c, 0x7197, 0x601e, 0x14a1,
		0x0528, 0x37b3, 0x263a, 0xdecd, 0xcf44, 0xfddf, 0xec56, 0x98e9, 0x8960,
		0xbbfb, 0xaa72, 0x6306, 0x728f, 0x4014, 0x519d, 0x2522, 0x34ab, 0x0630,
		0x17b9, 0xef4e, 0xfec7, 0xcc5c, 0xddd5, 0xa96a, 0xb8e3, 0x8a78, 0x9bf1,
		0x7387, 0x620e, 0x5095, 0x411c, 0x35a3, 0x242a, 0x16b1, 0x0738, 0xffcf,
		0xee46, 0xdcdd, 0xcd54, 0xb9eb, 0xa862, 0x9af9, 0x8b70, 0x8408, 0x9581,
		0xa71a, 0xb693, 0xc22c, 0xd3a5, 0xe13e, 0xf0b7, 0x0840, 0x19c9, 0x2b52,
		0x3adb, 0x4e64, 0x5fed, 0x6d76, 0x7cff, 0x9489, 0x8500, 0xb79b, 0xa612,
		0xd2ad, 0xc324, 0xf1bf, 0xe036, 0x18c1, 0x0948, 0x3bd3, 0x2a5a, 0x5ee5,
		0x4f6c, 0x7df7, 0x6c7e, 0xa50a, 0xb483, 0x8618, 0x9791, 0xe32e, 0xf2a7,
		0xc03c, 0xd1b5, 0x2942, 0x38cb, 0x0a50, 0x1bd9, 0x6f66, 0x7eef, 0x4c74,
		0x5dfd, 0xb58b, 0xa402, 0x9699, 0x8710, 0xf3af, 0xe226, 0xd0bd, 0xc134,
		0x39c3, 0x284a, 0x1ad1, 0x0b58, 0x7fe7, 0x6e6e, 0x5cf5, 0x4d7c, 0xc60c,
		0xd785, 0xe51e, 0xf497, 0x8028, 0x91a1, 0xa33a, 0xb2b3, 0x4a44, 0x5bcd,
		0x6956, 0x78df, 0x0c60, 0x1de9, 0x2f72, 0x3efb, 0xd68d, 0xc704, 0xf59f,
		0xe416, 0x90a9, 0x8120, 0xb3bb, 0xa232, 0x5ac5, 0x4b4c, 0x79d7, 0x685e,
		0x1ce1, 0x0d68, 0x3ff3, 0x2e7a, 0xe70e, 0xf687, 0xc41c, 0xd595, 0xa12a,
		0xb0a3, 0x8238, 0x93b1, 0x6b46, 0x7acf, 0x4854, 0x59dd, 0x2d62, 0x3ceb,
		0x0e70, 0x1ff9, 0xf78f, 0xe606, 0xd49d, 0xc514, 0xb1ab, 0xa022, 0x92b9,
		0x8330, 0x7bc7, 0x6a4e, 0x58d5, 0x495c, 0x3de3, 0x2c6a, 0x1ef1, 0x0f78
		};

		public void SetMaxBUfferSize(int MaxBUfferSize)
		{
			yahdlc_stateRx.MaxDataBufferSize = MaxBUfferSize;
		}
		public int GetMaxBUfferSize()
		{
			return yahdlc_stateRx.MaxDataBufferSize;
		}
		public int GetStationAddress()
		{
			return yahdlc_stateRx.StationAddress;
		}

		public int fcs16(int fcs, int value)
		{
			return (fcs >> 8) ^ fcstab[(fcs ^ value) & 0xff];
		}

		/*        {
                    .control_escape = 0,
                    .fcs = FCS16_INIT_VALUE,
                    .start_index = -1,
                    .end_index = -1,
                    .src_index = 0,
                    .dest_index = 0,
                }*/
		public unsafe int yahdlc_set_state(Yahdlc_state_t* state)
		{
			/* if (!state)
            {
        return -EINVAL;
            }*/

			yahdlc_state = *state;
			return 0;
		}
		public unsafe int yahdlc_get_state(Yahdlc_state_t* state)
		{
			/*if (!state)
            {
                return -EINVAL;
            }*/

			*state = yahdlc_state;
			return 0;
		}
		public unsafe void yahdlc_escape_value(int value, int* dest, int* dest_index)
		{
			// Check and escape the value if needed
			if ((value == YAHDLC_FLAG_SEQUENCE) || (value == YAHDLC_CONTROL_ESCAPE))
			{
				dest[(*dest_index)++] = YAHDLC_CONTROL_ESCAPE;
				value ^= 0x20;
			}

			// Add the value to the destination buffer and increment destination index
			dest[(*dest_index)++] = value;
		}
		public unsafe void yahdlc_escape_stxetx_value(int value, int* dest, int* dest_index)
		{
			// Check and escape the value if needed
			if ((value == YAHDLC_FLAG_SEQUENCE) ||
				(value == YAHDLC_CONTROL_ESCAPE) ||
				(value == YAHDLC_START_FLAG_SEQUENCE) ||
				(value == YAHDLC_END_FLAG_SEQUENCE))
			{
				dest[(*dest_index)++] = YAHDLC_CONTROL_ESCAPE;
				value ^= 0x20;
			}

			// Add the value to the destination buffer and increment destination index
			dest[(*dest_index)++] = value;
		}

		public unsafe yahdlc_control_t yahdlc_get_control_type(int control)
		{
			yahdlc_control_t value = new yahdlc_control_t();
			int Myresult;
			int MyShort;

			// Check if the frame is a S-frame (or U-frame)
			Myresult = control & (1 << YAHDLC_CONTROL_S_OR_U_FRAME_BIT);
			if (Myresult != 0)
			{

			}
			if ((control & (1 << YAHDLC_CONTROL_S_OR_U_FRAME_BIT)) != 0)
			{
				// Check if S-frame type is a Receive Ready (ACK)
				if (((control >> YAHDLC_CONTROL_S_FRAME_TYPE_BIT) & 0x3)
					== YAHDLC_CONTROL_TYPE_RECEIVE_READY)
				{
					value.frame = yahdlc_frame_t.YAHDLC_FRAME_ACK;
				}
				else
				{
					// Assume it is an NACK since Receive Not Ready, Selective Reject and U-frames are not supported
					value.frame = yahdlc_frame_t.YAHDLC_FRAME_NACK;
				}

				// Add the receive sequence number from the S-frame (or U-frame)
				MyShort = control;
				MyShort >>= YAHDLC_CONTROL_RECV_SEQ_NO_BIT;
				value.seq_no = MyShort;//(short)control;// >> (short)YAHDLC_CONTROL_RECV_SEQ_NO_BIT;
			}
			else
			{
				// It must be an I-frame so add the send sequence number (receive sequence number is not used)
				value.frame = yahdlc_frame_t.YAHDLC_FRAME_DATA;
				MyShort = control;
				MyShort >>= YAHDLC_CONTROL_RECV_SEQ_NO_BIT;
				value.seq_no = MyShort;// control >> YAHDLC_CONTROL_SEND_SEQ_NO_BIT;
			}

			return value;
		}
		public unsafe int yahdlc_frame_control_type(yahdlc_control_t* control)
		{
			int value = 0;
			int tmpShort = 0;

			// For details see: https://en.wikipedia.org/wiki/High-Level_Data_Link_Control
			switch (control->frame)
			{
				case yahdlc_frame_t.YAHDLC_FRAME_DATA:
					// Create the HDLC I-frame control byte with Poll bit set
					value = (control->seq_no);
					value <<= YAHDLC_CONTROL_SEND_SEQ_NO_BIT;
					//value |= (control->seq_no << YAHDLC_CONTROL_SEND_SEQ_NO_BIT);
					value |= (1 << YAHDLC_CONTROL_POLL_BIT);
					break;
				case yahdlc_frame_t.YAHDLC_FRAME_ACK:
					// Create the HDLC Receive Ready S-frame control byte with Poll bit cleared
					value = (control->seq_no);
					value <<= YAHDLC_CONTROL_SEND_SEQ_NO_BIT;
					//                    value |= (control->seq_no << YAHDLC_CONTROL_RECV_SEQ_NO_BIT);
					value |= (1 << YAHDLC_CONTROL_S_OR_U_FRAME_BIT);
					break;
				case yahdlc_frame_t.YAHDLC_FRAME_NACK:
					// Create the HDLC Receive Ready S-frame control byte with Poll bit cleared
					value = (control->seq_no);
					value <<= YAHDLC_CONTROL_SEND_SEQ_NO_BIT;
					tmpShort = YAHDLC_CONTROL_TYPE_REJECT;
					tmpShort <<= YAHDLC_CONTROL_S_FRAME_TYPE_BIT;
					value |= tmpShort;
					//                   value |= (control->seq_no << YAHDLC_CONTROL_RECV_SEQ_NO_BIT);
					//value |= (YAHDLC_CONTROL_TYPE_REJECT << YAHDLC_CONTROL_S_FRAME_TYPE_BIT);
					value |= (1 << YAHDLC_CONTROL_S_OR_U_FRAME_BIT);
					break;
			}

			return value;
		}
		public unsafe void yahdlc_get_data_reset()
		{
			yahdlc_state.fcs = FCS16_INIT_VALUE;
			yahdlc_state.start_index = yahdlc_state.end_index = -1;
			yahdlc_state.src_index = yahdlc_state.dest_index = 0;
			yahdlc_state.control_escape = 0;
		}
		public unsafe void yahdlc_get_data_resetRx()
		{
			yahdlc_stateRx.fcs = FCS16_INIT_VALUE;
			yahdlc_stateRx.start_index = yahdlc_stateRx.end_index = -1;
			yahdlc_stateRx.src_index = yahdlc_stateRx.dest_index = 0;
			yahdlc_stateRx.control_escape = 0;
			yahdlc_stateRx.AdditionalFlagSequenceDetectedFlag = 0;
			yahdlc_stateRx.EndSeqAdditionalFlagSequenceDetectedFlag = 0;
			yahdlc_stateRx.MaxDataBufferSize = 800; // Set a defualt value 
		}

		//       public unsafe void yahdlc_get_data_reset_with_state(yahdlc_state_t* state)
		/*public unsafe void yahdlc_get_data_reset_with_state()
        {
            /*           state->fcs              = FCS16_INIT_VALUE;
                       state->start_index      = state->end_index = -1;
                       state->src_index        = state->dest_index = 0;
                       state->control_escape   = 0;
            yahdlc_state.fcs                = FCS16_INIT_VALUE;
            yahdlc_state.start_index        = yahdlc_state.end_index = -1;
            yahdlc_state.src_index          = yahdlc_state.dest_index = 0;
            yahdlc_state.control_escape     = 0;
        }*/
		public unsafe void yahdlc_get_data_reset_with_state(Yahdlc_state_t* state)
		{
			state->fcs = FCS16_INIT_VALUE;
			state->start_index = state->end_index = -1;
			state->src_index = state->dest_index = 0;
			state->control_escape = 0;
		}

		public unsafe int yahdlc_get_data(yahdlc_control_t* control, int* src,
								 int src_len, int* dest, int* dest_len)
		{
			//return yahdlc_get_data_with_state(&yahdlc_state, control, src, src_len, dest, dest_len);
			return 0;
		}

		public unsafe int yahdlc_get_data_with_state(Yahdlc_state_t* state, yahdlc_control_t* control, int* src,
							 int src_len, int* dest, int* dest_len)
		{
			int ret;
			int value;
			int i;

			// Make sure that all parameters are valid
			/* if (!state || !control || !src || !dest || !dest_len)
             {
                 return -EINVAL;
             }*/

			// Run through the data bytes
			for (i = 0; i < src_len; i++)
			{
				// First find the start flag sequence
				if (state->start_index < 0)
				{
					if (src[i] == YAHDLC_FLAG_SEQUENCE)
					{
						// Check if an additional flag sequence byte is present
						if ((i < (src_len - 1)) && (src[i + 1] == YAHDLC_FLAG_SEQUENCE))
						{
							// Just loop again to silently discard it (accordingly to HDLC)
							continue;
						}

						state->start_index = state->src_index;
					}
				}
				else
				{
					// Check for end flag sequence
					if (src[i] == YAHDLC_FLAG_SEQUENCE)
					{
						// Check if an additional flag sequence byte is present or earlier received
						if (((i < (src_len - 1)) && (src[i + 1] == YAHDLC_FLAG_SEQUENCE))
							|| ((state->start_index + 1) == state->src_index))
						{
							// Just loop again to silently discard it (accordingly to HDLC)
							continue;
						}

						state->end_index = state->src_index;
						break;
					}
					else if (src[i] == YAHDLC_CONTROL_ESCAPE)
					{
						state->control_escape = 1;
					}
					else
					{
						// Update the value based on any control escape received
						if (state->control_escape > 0)
						{
							state->control_escape = 0;
							value = src[i] ^ 0x20;
						}
						else
						{
							value = src[i];
						}

						// Now update the FCS value
						state->fcs = fcs16(state->fcs, value);

						if (state->src_index == state->start_index + 2)
						{
							// Control field is the second byte after the start flag sequence
							*control = yahdlc_get_control_type(value);
						}
						else if (state->src_index > (state->start_index + 2))
						{
							// Start adding the data values after the Control field to the buffer
							dest[state->dest_index++] = value;
						}
					}
				}
				state->src_index++;
			}

			// Check for invalid frame (no start or end flag sequence)
			if ((state->start_index < 0) || (state->end_index < 0))
			{
				// Return no message and make sure destination length is 0
				*dest_len = 0;
				ret = -ENOMSG;
			}
			else
			{
				// A frame is at least 4 bytes in size and has a valid FCS value
				if ((state->end_index < (state->start_index + 4))
					|| (state->fcs != FCS16_GOOD_VALUE))
				{
					// Return FCS error and indicate that data up to end flag sequence in buffer should be discarded
					*dest_len = i;
					ret = -EIO;
				}
				else
				{
					// Return success and indicate that data up to end flag sequence in buffer should be discarded
					unsafe
					{
						*dest_len = state->dest_index - 2; //- LMN sizeof(state->fcs);
						ret = i;
					}
				}

				// Reset values for next frame
				yahdlc_get_data_reset_with_state(state);
				//yahdlc_get_data_reset_with_state();
			}

			return ret;
		} // END public unsafe int yahdlc_get_data_with_state
		  //        public unsafe int yahdlc_get_data_with_state_rentrant(yahdlc_state_t* state, yahdlc_control_t* control, int* src,
		  //                             int src_len, int* dest, int* dest_len)
		public unsafe int Yahdlc_get_data_with_state_rentrant(int* src, int src_len, int* dest, int* dest_len)
		{
			int ret = 1;
			int value;
			int i;

			// Make sure that all parameters are valid
			/* if (!state || !control || !src || !dest || !dest_len)
             {
                 return -EINVAL;
             }*/

			// Run through the data bytes
			for (i = 0; i < src_len; i++)
			{
				// First find the start flag sequence
				if (yahdlc_stateRx.start_index < 0)
				{
					if (yahdlc_stateRx.AdditionalFlagSequenceDetectedFlag == 1)
					{
						yahdlc_stateRx.AdditionalFlagSequenceDetectedFlag = 0;
						if (src[i] == YAHDLC_FLAG_SEQUENCE)
						{
							continue;
						}
					}
					else if (src[i] == YAHDLC_FLAG_SEQUENCE)
					{
						yahdlc_stateRx.AdditionalFlagSequenceDetectedFlag = 1;
						yahdlc_stateRx.start_index = yahdlc_stateRx.src_index;
					}
					/*
                                        if (src[i] == YAHDLC_FLAG_SEQUENCE)
                                        {
                                            // Check if an additional flag sequence byte is present
                                            if ((i < (src_len - 1)) && (src[i + 1] == YAHDLC_FLAG_SEQUENCE))
                                            {
                                                // Just loop again to silently discard it (accordingly to HDLC)
                                                continue;
                                            }

                                            yahdlc_stateRx.start_index = yahdlc_stateRx.src_index;
                                        }
                    */
				}
				else
				{
					// Check for end flag sequence
					if (yahdlc_stateRx.EndSeqAdditionalFlagSequenceDetectedFlag == 1)
					{
						yahdlc_stateRx.EndSeqAdditionalFlagSequenceDetectedFlag = 0;
						if (src[i] == YAHDLC_FLAG_SEQUENCE)
						{
							continue;
						}
						else
						{
							// The FCS should check out now... if not abort reset and look for a start.
						}
					}
					else if (src[i] == YAHDLC_FLAG_SEQUENCE)
					{
						yahdlc_stateRx.EndSeqAdditionalFlagSequenceDetectedFlag = 1;
						yahdlc_stateRx.end_index = yahdlc_stateRx.src_index;
						break;
					}
					/*                   if (src[i] == YAHDLC_FLAG_SEQUENCE)
                                        {
                                            // Check if an additional flag sequence byte is present or earlier received
                                            if (((i < (src_len - 1)) && (src[i + 1] == YAHDLC_FLAG_SEQUENCE))
                                                || ((yahdlc_stateRx.start_index + 1) == yahdlc_stateRx.src_index))
                                            {
                                                // Just loop again to silently discard it (accordingly to HDLC)
                                                continue;
                                            }

                                            yahdlc_stateRx.end_index = yahdlc_stateRx.src_index;
                                            break;
                                    }
                    */
					else if (src[i] == YAHDLC_CONTROL_ESCAPE)
					{
						yahdlc_stateRx.control_escape = 1;
					}
					else
					{
						// Update the value based on any control escape received
						if (yahdlc_stateRx.control_escape > 0)
						{
							yahdlc_stateRx.control_escape = 0;
							value = src[i] ^ 0x20;
						}
						else
						{
							value = src[i];
						}

						// Now update the FCS value
						yahdlc_stateRx.fcs = fcs16(yahdlc_stateRx.fcs, value);

						if (yahdlc_stateRx.src_index == yahdlc_stateRx.start_index + 2)
						{
							// Control field is the second byte after the start flag sequence
							yahdlc_controlRx = yahdlc_get_control_type(value);
						}
						else if (yahdlc_stateRx.src_index > (yahdlc_stateRx.start_index + 2))
						{
							// Start adding the data values after the Control field to the buffer
							dest[yahdlc_stateRx.dest_index++] = value;
							if (yahdlc_stateRx.dest_index >= yahdlc_stateRx.MaxDataBufferSize)
							{
								// Abort max data length hit
								*dest_len = 0;
								ret = 2;
								//yahdlc_get_data_reset_with_state(state);
								yahdlc_get_data_resetRx();
							}
						}
					}
				}
				yahdlc_stateRx.src_index++;
			}

			// Check for invalid frame (no start or end flag sequence)
			//if ((state->start_index < 0) || (state->end_index < 0))
			if (yahdlc_stateRx.start_index < 0)
			{
				// Return no message and make sure destination length is 0
				*dest_len = 0;
				ret = 1;
				//yahdlc_get_data_reset_with_state(state);
				yahdlc_get_data_resetRx();
			}
			else
			{
				// A frame is at least 4 bytes in size and has a valid FCS value
				if (yahdlc_stateRx.fcs == FCS16_GOOD_VALUE)
				{
					// Return success and indicate that data up to end flag sequence in buffer should be discarded
					unsafe
					{
						*dest_len = yahdlc_stateRx.dest_index - 2; //- LMN sizeof(state->fcs);
						ret = 0;
						//yahdlc_get_data_reset_with_state(state);
						yahdlc_get_data_resetRx();
					}
				}
				else
				{
					if (yahdlc_stateRx.EndSeqAdditionalFlagSequenceDetectedFlag == 1)
					{
						// End sequence but not a valid FCS... abort
						*dest_len = 0;
						ret = 3;
						//yahdlc_get_data_reset_with_state(state);
						yahdlc_get_data_resetRx();
					}
				}

			}

			return ret;
		} // END public unsafe int yahdlc_get_data_with_state
		public unsafe int yahdlc_frame_data(yahdlc_control_t* control, int* src,
							  int src_len, int* dest, int* dest_len)
		{
			int i;
			int dest_index = 0;
			int value = 0;
			int fcs = FCS16_INIT_VALUE;

			// Make sure that all parameters are valid
			/*if (!control || (!src && (src_len > 0)) || !dest || !dest_len) 
            {
                return -EINVAL;
            }*/

			// Start by adding the start flag sequence
			dest[dest_index++] = YAHDLC_FLAG_SEQUENCE;

			// Add the all-station address from HDLC (broadcast)
			fcs = fcs16(fcs, YAHDLC_ALL_STATION_ADDR);
			yahdlc_escape_value(YAHDLC_ALL_STATION_ADDR, dest, &dest_index);

			// Add the framed control field value
			value = yahdlc_frame_control_type(control);
			fcs = fcs16(fcs, value);
			yahdlc_escape_value(value, dest, &dest_index);

			// Only DATA frames should contain data
			if (control->frame == yahdlc_frame_t.YAHDLC_FRAME_DATA)
			{
				// Calculate FCS and escape data
				for (i = 0; i < src_len; i++)
				{
					fcs = fcs16(fcs, src[i]);
					yahdlc_escape_value(src[i], dest, &dest_index);
				}
			}

			// Invert the FCS value accordingly to the specification
			fcs ^= 0xFFFF;

			// Run through the FCS bytes and escape the values
			//LMN       for (i = 0; i< sizeof(fcs); i++) 
			for (i = 0; i < 2; i++)  // CHECK THIS PLEASE
			{
				value = ((fcs >> (8 * i)) & 0xFF);
				yahdlc_escape_value(value, dest, &dest_index);
			}

			// Add end flag sequence and update length of frame
			dest[dest_index++] = YAHDLC_FLAG_SEQUENCE;
			*dest_len = dest_index;

			return 0;
		} // EDN  int yahdlc_frame_data
		public unsafe int yahdlc_get_data_with_stx_etx(int* src, int src_len, int* dest, int* dest_len)
		{
			int ret = 1;
			int value;
			int i;


			// Run through the data bytes
			for (i = 0; i < src_len; i++)
			{
				if (i == 94)
				{
					i = 94;
				}
				// First find the start flag sequence
				if (src[i] == YAHDLC_START_FLAG_SEQUENCE)
				{
					yahdlc_get_data_resetRx();
					yahdlc_stateRx.AdditionalFlagSequenceDetectedFlag = 1;
					yahdlc_stateRx.start_index = yahdlc_stateRx.src_index;
				}
				else if (src[i] == YAHDLC_CONTROL_ESCAPE)
				{
					yahdlc_stateRx.control_escape = 1;
				}
				else if (src[i] == YAHDLC_END_FLAG_SEQUENCE)
				{
					yahdlc_stateRx.control_escape = 1;
					if (yahdlc_stateRx.fcs == FCS16_GOOD_VALUE)
					{
						// Return success and indicate that data up to end flag sequence in buffer should be discarded
						*dest_len = yahdlc_stateRx.dest_index - 2;
						ret = 0;
						yahdlc_get_data_resetRx();
					}
					else
					{
						// End sequence but not a valid FCS... abort
						*dest_len = 0;
						ret = 3;
						yahdlc_get_data_resetRx();
					}

				}
				else
				{
					// Update the value based on any control escape received
					if (yahdlc_stateRx.control_escape == 1)
					{
						yahdlc_stateRx.control_escape = 0;
						value = src[i] ^ 0x20;
					}
					else
					{
						value = src[i];
					}

					// Now update the FCS value
					yahdlc_stateRx.fcs = fcs16(yahdlc_stateRx.fcs, value);

					if (yahdlc_stateRx.src_index == yahdlc_stateRx.start_index + 1)
					{
						// Control field is the second byte after the start flag sequence
						yahdlc_stateRx.StationAddress = value;
					}
					if (yahdlc_stateRx.src_index == yahdlc_stateRx.start_index + 2)
					{
						// Control field is the second byte after the start flag sequence
						//* control = yahdlc_get_control_type(value);
					}
					else if (yahdlc_stateRx.src_index > (yahdlc_stateRx.start_index + 2))
					{
						// Start adding the data values after the Control field to the buffer
						dest[yahdlc_stateRx.dest_index++] = value;
						if (yahdlc_stateRx.dest_index >= yahdlc_stateRx.MaxDataBufferSize)
						{
							// Abort max data length hit
							*dest_len = 0;
							ret = 2;
							//yahdlc_get_data_reset_with_state(state);
							yahdlc_get_data_resetRx();
						}
					}
				}
				yahdlc_stateRx.src_index++;
			}

			return ret;
		}

		public unsafe int yahdlc_frame_stx_etx_data(int StationAddress, yahdlc_control_t* control, int* src, int src_len, int* dest, int* dest_len)
		{
			int i;
			int dest_index = 0;
			int value = 0;
			int fcs = FCS16_INIT_VALUE;

			// Start by adding the start flag sequence
			dest[dest_index++] = YAHDLC_START_FLAG_SEQUENCE;

			// Add the all-station address from HDLC (broadcast)
			fcs = fcs16(fcs, StationAddress);
			yahdlc_escape_value(StationAddress, dest, &dest_index);

			// Add the framed control field value
			value = yahdlc_frame_control_type(control);
			fcs = fcs16(fcs, value);
			yahdlc_escape_value(value, dest, &dest_index);

			// Only DATA frames should contain data
			if (control->frame == yahdlc_frame_t.YAHDLC_FRAME_DATA)
			{
				// Calculate FCS and escape data
				for (i = 0; i < src_len; i++)
				{
					fcs = fcs16(fcs, src[i]);
					yahdlc_escape_stxetx_value(src[i], dest, &dest_index);
				}
			}

			// Invert the FCS value accordingly to the specification
			fcs ^= 0xFFFF;

			// Run through the FCS bytes and escape the values
			for (i = 0; i < 2; i++)
			{
				value = ((fcs >> (8 * i)) & 0xFF);
				yahdlc_escape_stxetx_value(value, dest, &dest_index);
			}

			// Add end flag sequence and update length of frame
			dest[dest_index++] = YAHDLC_END_FLAG_SEQUENCE;
			*dest_len = dest_index;

			return 0;
		} // END yahdlc_frame_stx_etx_data


	} // END PUBLIC HDLC
}
